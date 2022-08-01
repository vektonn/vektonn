using System;
using System.Linq;
using Confluent.Kafka;

namespace Vektonn.DataSource.Kafka
{
    public class KafkaConsumerConfig
    {
        public KafkaConsumerConfig(string[] bootstrapServers)
        {
            if (!bootstrapServers.Any())
                throw new InvalidOperationException($"{nameof(bootstrapServers)} is empty");

            BootstrapServers = bootstrapServers;
        }

        public string[] BootstrapServers { get; }
        public TimeSpan TopicMetadataRefreshInterval { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan MaxFetchDelay { get; set; } = TimeSpan.FromMilliseconds(100);
        public TimeSpan MinRetryDelay { get; set; } = TimeSpan.FromMilliseconds(100);
        public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan WatermarkOffsetsQueryTimeout { get; set; } = TimeSpan.FromSeconds(1);
        public int ConsumeBatchSize { get; set; } = 10_000;
        public Action<ConsumerConfig> ConsumerConfigUserTuning { get; set; } = config => {};

        public void ConsumerConfigTuning(ConsumerConfig consumerConfig)
        {
            consumerConfig.BootstrapServers = string.Join(",", BootstrapServers);
            consumerConfig.GroupId = $"Vektonn-{Guid.NewGuid():N}";
            consumerConfig.EnableAutoCommit = false;
            consumerConfig.AutoCommitIntervalMs = 0;
            consumerConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
            consumerConfig.FetchWaitMaxMs = (int)MaxFetchDelay.TotalMilliseconds;
            consumerConfig.TopicMetadataRefreshIntervalMs = (int)TopicMetadataRefreshInterval.TotalMilliseconds;
        }
    }
}
