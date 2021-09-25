using System;
using System.Collections.Generic;
using Confluent.Kafka.Admin;

namespace Vektonn.DataSource.Kafka
{
    public class KafkaTopicCreationConfig
    {
        public KafkaTopicCreationConfig(byte topicReplicationFactor)
        {
            TopicReplicationFactor = topicReplicationFactor;
        }

        public byte TopicReplicationFactor { get; }
        public uint TopicSegmentBytes { get; set; } = 268_435_456;
        public double TopicMinCleanableDirtyRatio { get; set; } = 0.1;
        public TimeSpan TopicDeleteRetention { get; set; } = TimeSpan.FromDays(1);

        public TopicSpecification BuildTopicSpecification(string topicName)
        {
            return new TopicSpecification
            {
                Name = topicName,
                NumPartitions = 1,
                ReplicationFactor = TopicReplicationFactor,
                Configs = new Dictionary<string, string>
                {
                    ["cleanup.policy"] = "compact",
                    ["segment.bytes"] = TopicSegmentBytes.ToString("D"),
                    ["min.cleanable.dirty.ratio"] = TopicMinCleanableDirtyRatio.ToString("F"),
                    ["delete.retention.ms"] = ((long)TopicDeleteRetention.TotalMilliseconds).ToString("D"),
                }
            };
        }
    }
}
