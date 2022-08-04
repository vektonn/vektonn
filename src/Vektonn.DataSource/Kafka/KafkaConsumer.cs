using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Vektonn.SharedImpl.Json;
using Vostok.Logging.Abstractions;

namespace Vektonn.DataSource.Kafka
{
    // todo (andrew, 25.09.2021): test
    // todo (andrew, 01.10.2021): debug logs
    internal class KafkaConsumer : IDisposable
    {
        private readonly ILog log;
        private readonly KafkaConsumerConfig kafkaConsumerConfig;
        private readonly string[] topicsToConsume;
        private readonly Action<IReadOnlyList<Message<byte[], byte[]>>> processKafkaMessages;
        private readonly CommittedOffsets committedOffsets;
        private readonly IConsumer<byte[], byte[]> consumer;
        private readonly ManualResetEventSlim firstAssignmentSignal = new();
        private Thread? consumerLoopThread;
        private int unknownTopicErrors;

        public KafkaConsumer(
            ILog log,
            KafkaConsumerConfig kafkaConsumerConfig,
            string[] topicsToConsume,
            Action<IReadOnlyList<Message<byte[], byte[]>>> processKafkaMessages)
        {
            if (!topicsToConsume.Any())
                throw new InvalidOperationException($"{nameof(topicsToConsume)} is empty");

            this.log = log.ForContext<KafkaConsumer>();
            this.kafkaConsumerConfig = kafkaConsumerConfig;
            this.topicsToConsume = topicsToConsume;
            this.processKafkaMessages = processKafkaMessages;

            committedOffsets = new CommittedOffsets(this.log);

            var consumerConfig = new ConsumerConfig();
            kafkaConsumerConfig.CustomizeConsumerConfig(consumerConfig);
            kafkaConsumerConfig.CustomizeConsumerConfigInternal(consumerConfig);

            consumer = BuildConsumer(consumerConfig);
        }

        public void Dispose()
        {
            consumerLoopThread?.Join();
            consumer.Close();
            consumer.Dispose();
            firstAssignmentSignal.Dispose();
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            log.Info($"Subscribing to topics: {string.Join("; ", topicsToConsume)}");
            consumer.Subscribe(topicsToConsume);

            var initializationTcs = new TaskCompletionSource();

            // note (andrew, 30.09.2021): using Thread.Start() instead of Task.Run() to trigger process crash on unhandled exception in update loop
            consumerLoopThread = new Thread(() => ConsumeLoopAsync(initializationTcs, cancellationToken).GetAwaiter().GetResult())
            {
                IsBackground = true,
                Name = nameof(KafkaConsumer),
            };
            consumerLoopThread.Start();

            await initializationTcs.Task;
        }

        private IConsumer<byte[], byte[]> BuildConsumer(ConsumerConfig consumerConfig)
        {
            var consumerBuilder = new ConsumerBuilder<byte[], byte[]>(consumerConfig)
                .SetKeyDeserializer(Deserializers.ByteArray)
                .SetValueDeserializer(Deserializers.ByteArray)
                .SetErrorHandler(
                    (_, error) => LogExtensions.Error(this.log, $"ConfluentConsumer error: {error.ToPrettyJson()}"))
                .SetLogHandler(
                    (_, logMessage) => LogExtensions.Debug(this.log, $"ConfluentConsumer logMessage: {logMessage.ToPrettyJson()}"))
                .SetPartitionsAssignedHandler(
                    (_, topicPartitions) =>
                    {
                        var offsetsToConsumeFrom = committedOffsets.GetOffsetsToConsumeFrom(topicPartitions);
                        LogExtensions.Info(this.log, $"Assigned {topicPartitions.Count} partitions with offsetsToConsumeFrom: {string.Join("; ", offsetsToConsumeFrom.Select(x => x.ToString()))}");
                        firstAssignmentSignal.Set();
                        return offsetsToConsumeFrom;
                    });
            return consumerBuilder.Build();
        }

        private async Task ConsumeLoopAsync(TaskCompletionSource initializationTcs, CancellationToken cancellationToken)
        {
            try
            {
                var warmupIsNeeded = true;
                while (!firstAssignmentSignal.IsSet)
                {
                    await ConsumeBatchAsync(cancellationToken);

                    // todo (andrew, 15.10.2021): handle topic subscriptions by pattern
                    if (unknownTopicErrors == topicsToConsume.Length)
                    {
                        warmupIsNeeded = false;
                        log.Warn("Seems like there is no data to consume, skipping warmup phase");
                        break;
                    }
                }

                if (warmupIsNeeded)
                {
                    var watermarkOffsets = GetWatermarkOffsets();
                    log.Info($"Got watermark offsets: {watermarkOffsets}");

                    while (!committedOffsets.HasReached(watermarkOffsets))
                        await ConsumeBatchAsync(cancellationToken);

                    CollectGarbageWithLohCompaction();
                }

                log.Info("Initialization completed");
                initializationTcs.SetResult();

                while (true)
                    await ConsumeBatchAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                initializationTcs.TrySetCanceled(cancellationToken);
            }
        }

        private KafkaTopicPartitionOffsets GetWatermarkOffsets()
        {
            var watermarkOffsets = consumer.Assignment
                .Select(
                    topicPartition => (
                        TopicPartition: topicPartition,
                        WatermarkOffsets: consumer.QueryWatermarkOffsets(topicPartition, kafkaConsumerConfig.WatermarkOffsetsQueryTimeout)
                    ))
                .ToDictionary(t => t.TopicPartition, t => t.WatermarkOffsets);

            return KafkaTopicPartitionOffsets.FromWatermarkOffsets(watermarkOffsets);
        }

        private static void CollectGarbageWithLohCompaction()
        {
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(generation: 2, GCCollectionMode.Forced, blocking: true, compacting: true);
        }

        private async Task ConsumeBatchAsync(CancellationToken cancellationToken)
        {
            var kafkaMessages = new List<Message<byte[], byte[]>>();
            var consumedOffsets = KafkaTopicPartitionOffsets.Empty();

            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < kafkaConsumerConfig.MaxFetchDelay && kafkaMessages.Count < kafkaConsumerConfig.ConsumeBatchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var consumeResult = await ConsumeAsync(cancellationToken);
                if (consumeResult?.Message == null)
                    continue;

                if (consumedOffsets.TryAdvanceTo(consumeResult.TopicPartition, consumeResult.Offset, log))
                    kafkaMessages.Add(consumeResult.Message);
            }

            if (kafkaMessages.Any())
            {
                processKafkaMessages(kafkaMessages);

                committedOffsets.AdvanceTo(consumedOffsets);
            }
        }

        private async Task<ConsumeResult<byte[], byte[]>?> ConsumeAsync(CancellationToken cancellationToken)
        {
            var retryDelay = kafkaConsumerConfig.MinRetryDelay;

            while (true)
            {
                try
                {
                    return consumer.Consume(kafkaConsumerConfig.MaxFetchDelay);
                }
                catch (ConsumeException e)
                {
                    if (e.Error.IsFatal)
                    {
                        log.Fatal(e, $"ConfluentConsumer fatal error for TopicPartitionOffset {e.ConsumerRecord?.TopicPartitionOffset}: {e.Error.ToPrettyJson()}");
                        throw;
                    }

                    log.Warn(e, $"ConfluentConsumer transient error for TopicPartitionOffset {e.ConsumerRecord?.TopicPartitionOffset}: {e.Error.ToPrettyJson()}");

                    if (e.Error.Code == ErrorCode.UnknownTopicOrPart && e.Error.IsBrokerError)
                        ++unknownTopicErrors;

                    if (retryDelay < kafkaConsumerConfig.MaxRetryDelay)
                        retryDelay *= 2;

                    await Task.Delay(retryDelay, cancellationToken);
                }
            }
        }

        private class CommittedOffsets
        {
            private readonly ILog log;
            private readonly KafkaTopicPartitionOffsets offsets = KafkaTopicPartitionOffsets.Empty();

            public CommittedOffsets(ILog log)
            {
                this.log = log;
            }

            public void AdvanceTo(KafkaTopicPartitionOffsets consumedOffsets)
            {
                lock (this)
                    offsets.AdvanceTo(consumedOffsets, log);
            }

            public bool HasReached(KafkaTopicPartitionOffsets watermarkOffsets)
            {
                lock (this)
                    return offsets.HasReached(watermarkOffsets);
            }

            public TopicPartitionOffset[] GetOffsetsToConsumeFrom(IReadOnlyList<TopicPartition> topicPartitions)
            {
                lock (this)
                {
                    return topicPartitions
                        .Select(
                            topicPartition =>
                            {
                                var offset = offsets.TryGetOffset(topicPartition, out var currentOffset)
                                    ? new Offset(currentOffset + 1)
                                    : Offset.Beginning;

                                return new TopicPartitionOffset(topicPartition, offset);
                            })
                        .ToArray();
                }
            }
        }
    }
}
