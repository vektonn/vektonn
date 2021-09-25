using System;
using System.Collections.Generic;
using System.Linq;
using Confluent.Kafka;
using Vostok.Logging.Abstractions;

namespace Vektonn.DataSource.Kafka
{
    // todo (andrew, 25.09.2021): test
    public class KafkaTopicPartitionOffsets
    {
        private readonly Dictionary<TopicPartition, long> offsets;

        public KafkaTopicPartitionOffsets(Dictionary<TopicPartition, long> offsets)
        {
            this.offsets = offsets;
        }

        public static KafkaTopicPartitionOffsets Empty()
        {
            return new KafkaTopicPartitionOffsets(new Dictionary<TopicPartition, long>());
        }

        public static KafkaTopicPartitionOffsets FromWatermarkOffsets(Dictionary<TopicPartition, WatermarkOffsets> watermarkOffsets)
        {
            var offsets = watermarkOffsets
                .Where(pair => pair.Value.High > pair.Value.Low && pair.Value.High > 0)
                .ToDictionary(e => e.Key, e => e.Value.High - 1);

            return new KafkaTopicPartitionOffsets(offsets);
        }

        public bool HasReached(KafkaTopicPartitionOffsets other)
        {
            return other.offsets.All(e => offsets.ContainsKey(e.Key) && offsets[e.Key] >= e.Value);
        }

        public void AdvanceTo(KafkaTopicPartitionOffsets other, ILog log)
        {
            foreach (var (topicPartition, offset) in other.offsets)
                TryAdvanceTo(topicPartition, offset, log);
        }

        public bool TryAdvanceTo(TopicPartition topicPartition, Offset newOffset, ILog log)
        {
            if (newOffset.IsSpecial)
                throw new InvalidOperationException($"NewOffset for {topicPartition} is special: {newOffset}");

            if (offsets.TryGetValue(topicPartition, out var currentOffset) && currentOffset >= newOffset)
            {
                log.Warn($"Skipping offset @{newOffset} for {topicPartition} since currentOffset @{currentOffset} is already greater");
                return false;
            }

            offsets[topicPartition] = newOffset;
            return true;
        }

        public bool TryGetOffset(TopicPartition topicPartition, out long offset)
        {
            return offsets.TryGetValue(topicPartition, out offset);
        }

        public override string ToString()
        {
            return $"Offsets[{offsets.Count}]:\n\t{string.Join("\n\t", offsets.Select(t => $"{t.Key} @{t.Value}"))}";
        }
    }
}
