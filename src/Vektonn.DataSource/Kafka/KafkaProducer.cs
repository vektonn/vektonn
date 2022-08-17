using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Vektonn.SharedImpl.Json;
using Vostok.Logging.Abstractions;

namespace Vektonn.DataSource.Kafka
{
    // todo (andrew, 25.09.2021): test
    internal class KafkaProducer : IDisposable
    {
        private readonly ILog log;
        private readonly KafkaProducerConfig kafkaProducerConfig;
        private readonly TimeSpan topicMetadataRefreshInterval;
        private readonly IProducer<byte[], byte[]> producer;

        public KafkaProducer(ILog log, KafkaProducerConfig kafkaProducerConfig)
        {
            this.log = log.ForContext<KafkaProducer>();
            this.kafkaProducerConfig = kafkaProducerConfig;

            topicMetadataRefreshInterval = kafkaProducerConfig.ProduceTimeout.Multiply(0.3);

            var producerConfig = kafkaProducerConfig.GetConfluentProducerConfig(topicMetadataRefreshInterval);
            producer = BuildProducer(producerConfig);
        }

        public void Dispose()
        {
            var disposeTimeout = TimeSpan.FromSeconds(30);
            var librdkafkaOutQueueLength = producer.Flush(disposeTimeout);
            if (librdkafkaOutQueueLength > 0)
                log.Error($"Failed to flush producer in {disposeTimeout}. Current librdkafka out queue length: {librdkafkaOutQueueLength}");

            producer.Dispose();
        }

        public async Task ProduceAsync(string topicName, Message<byte[], byte[]>[] messages)
        {
            var success = await TryProduceAsync(topicName, messages, tolerateUnknownTopicError: true);
            if (success)
                return;

            await CreateTopicAsync(topicName);

            // note (andrew, 30.09.2021): wait for metadata cache invalidation in librdkafka instance bound to producer
            await Task.Delay(topicMetadataRefreshInterval);

            await TryProduceAsync(topicName, messages, tolerateUnknownTopicError: false);
        }

        private IProducer<byte[], byte[]> BuildProducer(ProducerConfig producerConfig)
        {
            return new ProducerBuilder<byte[], byte[]>(producerConfig)
                .SetKeySerializer(Serializers.ByteArray)
                .SetValueSerializer(Serializers.ByteArray)
                .SetErrorHandler(
                    (_, error) => LogExtensions.Error(log, $"ConfluentProducer error: {error.ToPrettyJson()}"))
                .SetLogHandler(
                    (_, logMessage) => LogExtensions.Debug(log, $"ConfluentProducer logMessage: {logMessage.ToPrettyJson()}"))
                .Build();
        }

        private async Task<bool> TryProduceAsync(string topicName, Message<byte[], byte[]>[] messages, bool tolerateUnknownTopicError)
        {
            var deliveryReportHandler = new DeliveryReportHandler(
                log,
                topicName,
                produceTimeout: kafkaProducerConfig.ProduceTimeout,
                messagesToProduce: messages.Length,
                tolerateUnknownTopicError);

            var topicPartition = new TopicPartition(topicName, new Partition(0));
            foreach (var message in messages)
                producer.Produce(topicPartition, message, deliveryReportHandler.Handle);

            var success = await deliveryReportHandler.EnsureSuccessAsync();

            return success;
        }

        private async Task CreateTopicAsync(string topicName)
        {
            // note (andrew, 30.09.2021):
            // we do not reuse single instance of KafkaAdminClient since it does not tolerate topic creation errors (e.g. due to broker timeouts)
            // see https://github.com/confluentinc/confluent-kafka-dotnet/issues/1491#issuecomment-923716823
            using var kafkaTopicCreator = new KafkaTopicCreator(log, kafkaProducerConfig);

            await kafkaTopicCreator.CreateTopicAsync(topicName, kafkaProducerConfig.ProduceTimeout);
        }

        private class DeliveryReportHandler
        {
            private readonly ILog log;
            private readonly string topicName;
            private readonly TimeSpan produceTimeout;
            private readonly int messagesToProduce;
            private readonly bool tolerateUnknownTopicError;
            private readonly ConcurrentBag<Error> errors = new();
            private int deliveryReportsToReceive;
            private int toleratedUnknownTopicErrors;

            public DeliveryReportHandler(
                ILog log,
                string topicName,
                TimeSpan produceTimeout,
                int messagesToProduce,
                bool tolerateUnknownTopicError)
            {
                this.log = log;
                this.topicName = topicName;
                this.produceTimeout = produceTimeout.Multiply(1.2);
                this.messagesToProduce = messagesToProduce;
                this.tolerateUnknownTopicError = tolerateUnknownTopicError;
                deliveryReportsToReceive = messagesToProduce;
            }

            public void Handle(DeliveryReport<byte[], byte[]> deliveryReport)
            {
                if (deliveryReport.Topic != topicName)
                    throw new InvalidOperationException($"Invalid topicName: {deliveryReport.Topic}. Expected: {topicName}");

                try
                {
                    if (!deliveryReport.Error.IsError)
                    {
                        if (deliveryReport.Status != PersistenceStatus.Persisted)
                            throw new InvalidOperationException($"There was no error but message was not persisted. Delivery report: {deliveryReport.ToPrettyJson()}");

                        return;
                    }

                    if (deliveryReport.Error.Code == ErrorCode.UnknownTopicOrPart && tolerateUnknownTopicError)
                    {
                        var toleratedUnknownTopicErrorsLocal = Interlocked.Increment(ref toleratedUnknownTopicErrors);

                        if (toleratedUnknownTopicErrorsLocal == 1)
                            log.Info($"Topic '{topicName}' does not exist. Will try to create it. DeliveryReport: {deliveryReport.ToPrettyJson()}");

                        return;
                    }

                    errors.Add(deliveryReport.Error);
                    log.Error($"Delivery failure report: {deliveryReport.ToPrettyJson()}");
                }
                finally
                {
                    Interlocked.Decrement(ref deliveryReportsToReceive);
                }
            }

            public async Task<bool> EnsureSuccessAsync()
            {
                var deliveryReportsToReceiveLocal = -1;
                for (var sw = Stopwatch.StartNew(); sw.Elapsed < produceTimeout;)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(10));

                    deliveryReportsToReceiveLocal = Interlocked.CompareExchange(ref deliveryReportsToReceive, 0, 0);
                    if (deliveryReportsToReceiveLocal == 0)
                        break;

                    if (deliveryReportsToReceiveLocal < 0)
                        throw new InvalidOperationException($"DeliveryReportsToReceive became negative: {deliveryReportsToReceiveLocal}");
                }

                var errorsString = string.Empty;
                var errorsLocal = errors.ToArray();
                if (errorsLocal.Any())
                    errorsString = $" Errors: {string.Join("; ", errorsLocal.Select(x => x.ToString()))}";

                if (deliveryReportsToReceiveLocal > 0 || !string.IsNullOrEmpty(errorsString))
                    throw new KafkaProducerException($"Failed to produce {messagesToProduce} messages in {produceTimeout}. DeliveryReportsToReceive: {deliveryReportsToReceiveLocal}.{errorsString}");

                var needToRetry = toleratedUnknownTopicErrors > 0;
                if (needToRetry && toleratedUnknownTopicErrors != messagesToProduce)
                {
                    // note (andrew, 30.09.2021): this may happen in case of concurrent calls to KafkaProducer.ProduceAsync() for the same topic
                    throw new KafkaProducerException($"Not all messages failed with UnknownTopic error. UnknownTopicErrors: {toleratedUnknownTopicErrors}, MessagesToProduce: {messagesToProduce}");
                }

                return !needToRetry;
            }
        }
    }
}
