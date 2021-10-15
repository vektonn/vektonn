using System;
using System.Linq;

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
    }
}
