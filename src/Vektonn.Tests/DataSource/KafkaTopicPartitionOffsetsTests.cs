using System.Collections.Generic;
using Confluent.Kafka;
using FluentAssertions;
using NUnit.Framework;
using Vektonn.DataSource.Kafka;
using Vostok.Logging.Abstractions;

namespace Vektonn.Tests.DataSource
{
    public class KafkaTopicPartitionOffsetsTests
    {
        private const string Topic = "topic";

        private static readonly TopicPartition TopicPartition0 = new(Topic, partition: 0);
        private static readonly TopicPartition TopicPartition1 = new(Topic, partition: 1);
        private static readonly TopicPartition TopicPartition2 = new(Topic, partition: 2);
        private static readonly TopicPartition TopicPartition3 = new(Topic, partition: 3);
        private static readonly TopicPartition TopicPartition4 = new(Topic, partition: 4);

        [Test]
        public void FromWatermarkOffsets()
        {
            var watermarkOffsets = new Dictionary<TopicPartition, WatermarkOffsets>
            {
                [TopicPartition0] = new(low: 0, high: 2),
                [TopicPartition1] = new(low: 0, high: 1),
                [TopicPartition2] = new(low: 1, high: 1),
                [TopicPartition3] = new(low: 0, high: 0),
                [TopicPartition4] = new(low: -2, high: -1)
            };

            var offsets = KafkaTopicPartitionOffsets.FromWatermarkOffsets(watermarkOffsets);

            offsets.TryGetOffset(TopicPartition0, out var offset).Should().BeTrue();
            offset.Should().Be(1);
            offsets.TryGetOffset(TopicPartition1, out offset).Should().BeTrue();
            offset.Should().Be(0);
            offsets.TryGetOffset(TopicPartition2, out _).Should().BeFalse();
            offsets.TryGetOffset(TopicPartition3, out _).Should().BeFalse();
            offsets.TryGetOffset(TopicPartition4, out _).Should().BeFalse();
        }

        [Test]
        public void AdvanceTo()
        {
            var first = new KafkaTopicPartitionOffsets(
                new Dictionary<TopicPartition, long> {[TopicPartition0] = 2, [TopicPartition1] = 4});
            var second = new KafkaTopicPartitionOffsets(
                new Dictionary<TopicPartition, long> {[TopicPartition1] = 6, [TopicPartition2] = 4});

            first.AdvanceTo(second, new SilentLog());

            first.TryGetOffset(TopicPartition0, out var offset).Should().BeTrue();
            offset.Should().Be(2);
            first.TryGetOffset(TopicPartition1, out offset).Should().BeTrue();
            offset.Should().Be(6);
            first.TryGetOffset(TopicPartition2, out offset).Should().BeTrue();
            offset.Should().Be(4);
        }

        [TestCaseSource(nameof(GetTestCases_Reach))]
        public bool HasReached(
            Dictionary<TopicPartition, long> offsetsDictionary,
            Dictionary<TopicPartition, long> offsetsToReachDictionary)
        {
            var offsets = new KafkaTopicPartitionOffsets(offsetsDictionary);
            var offsetsToReach = new KafkaTopicPartitionOffsets(offsetsToReachDictionary);

            return offsets.HasReached(offsetsToReach);
        }

        private static IEnumerable<TestCaseData> GetTestCases_Reach()
        {
            yield return new TestCaseData(
                    new Dictionary<TopicPartition, long>(),
                    new Dictionary<TopicPartition, long> {[TopicPartition0] = 4})
                .SetName("Empty current offsets")
                .Returns(false);

            yield return new TestCaseData(
                    new Dictionary<TopicPartition, long> {[TopicPartition0] = 2},
                    new Dictionary<TopicPartition, long> {[TopicPartition0] = 4})
                .SetName("One current partition with smaller offset")
                .Returns(false);

            yield return new TestCaseData(
                    new Dictionary<TopicPartition, long> {[TopicPartition0] = 4},
                    new Dictionary<TopicPartition, long> {[TopicPartition0] = 4})
                .SetName("One current partition with equal offset")
                .Returns(true);

            yield return new TestCaseData(
                    new Dictionary<TopicPartition, long> {[TopicPartition0] = 6},
                    new Dictionary<TopicPartition, long> {[TopicPartition0] = 4})
                .SetName("One current partition with greater offset")
                .Returns(true);

            yield return new TestCaseData(
                    new Dictionary<TopicPartition, long> {[TopicPartition0] = 6, [TopicPartition1] = 4},
                    new Dictionary<TopicPartition, long> {[TopicPartition0] = 4})
                .SetName("Two current partition reach another one partition")
                .Returns(true);

            yield return new TestCaseData(
                    new Dictionary<TopicPartition, long> {[TopicPartition0] = 6},
                    new Dictionary<TopicPartition, long> {[TopicPartition0] = 4, [TopicPartition1] = 6})
                .SetName("One current partition do not reach another two partitions")
                .Returns(false);

            yield return new TestCaseData(
                    new Dictionary<TopicPartition, long> {[TopicPartition0] = 6, [TopicPartition1] = 4},
                    new Dictionary<TopicPartition, long> {[TopicPartition0] = 4, [TopicPartition1] = 6})
                .SetName("Two current partitions do not reach another two partitions")
                .Returns(false);

            yield return new TestCaseData(
                    new Dictionary<TopicPartition, long> {[TopicPartition0] = 6, [TopicPartition1] = 6},
                    new Dictionary<TopicPartition, long> {[TopicPartition0] = 4, [TopicPartition1] = 6})
                .SetName("Two current partitions reach another two partitions")
                .Returns(true);
        }
    }
}
