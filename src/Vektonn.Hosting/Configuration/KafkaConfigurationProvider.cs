using System;

namespace Vektonn.Hosting.Configuration
{
    public class KafkaConfigurationProvider
    {
        public string[] GetKafkaBootstrapServers()
        {
            return EnvironmentVariables
                .Get("VEKTONN_KAFKA_BOOTSTRAP_SERVERS")
                .Split(';', StringSplitOptions.RemoveEmptyEntries);
        }

        public byte GetTopicReplicationFactor()
        {
            return EnvironmentVariables.Get("VEKTONN_KAFKA_TOPIC_REPLICATION_FACTOR", byte.Parse);
        }
    }
}
