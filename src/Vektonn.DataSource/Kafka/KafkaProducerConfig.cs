using System;
using System.Linq;
using Confluent.Kafka;

namespace Vektonn.DataSource.Kafka
{
    public class KafkaProducerConfig
    {
        public KafkaProducerConfig(string[] bootstrapServers, KafkaTopicCreationConfig topicCreationConfig)
        {
            if (!bootstrapServers.Any())
                throw new InvalidOperationException($"{nameof(bootstrapServers)} is empty");

            BootstrapServers = bootstrapServers;
            TopicCreationConfig = topicCreationConfig;
        }

        public string[] BootstrapServers { get; }
        public KafkaTopicCreationConfig TopicCreationConfig { get; }
        public TimeSpan ProduceTimeout { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan LingerDelay { get; set; } = TimeSpan.FromMilliseconds(10);
        public Action<ProducerConfig> CustomizeProducerConfig { get; set; } = config => {};

        internal void CustomizeProducerConfigInternal(ProducerConfig producerConfig, TimeSpan topicMetadataRefreshInterval)
        {
            producerConfig.BootstrapServers = string.Join(",", BootstrapServers);
            producerConfig.Acks = Acks.All;
            producerConfig.MessageSendMaxRetries = 0;
            producerConfig.LingerMs = LingerDelay.TotalMilliseconds;
            producerConfig.RequestTimeoutMs = (int)ProduceTimeout.TotalMilliseconds;
            producerConfig.MessageTimeoutMs = (int)ProduceTimeout.TotalMilliseconds;
            producerConfig.TopicMetadataPropagationMaxMs = (int)topicMetadataRefreshInterval.TotalMilliseconds;
            producerConfig.TopicMetadataRefreshIntervalMs = (int)topicMetadataRefreshInterval.TotalMilliseconds;
        }
    }
}
