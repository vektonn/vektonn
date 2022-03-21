using System;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using Vektonn.SharedImpl.Contracts;
using Vektonn.SharedImpl.Contracts.Sharding.Index;
using static Vektonn.Tests.AttributeValueTestHelpers;

namespace Vektonn.Tests.SharedImpl.Sharding
{
    public class IndexShardMetaTests
    {
        private IIndexAttributeValueShard trueShard = null!;
        private IIndexAttributeValueShard falseShard = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            trueShard = A.Fake<IIndexAttributeValueShard>();
            A.CallTo(() => trueShard.Contains(A<AttributeValue>.That.IsEqualTo(AttributeValue(true)))).Returns(true);
            A.CallTo(() => trueShard.Contains(A<AttributeValue>.That.IsEqualTo(AttributeValue(false)))).Returns(false);

            falseShard = A.Fake<IIndexAttributeValueShard>();
            A.CallTo(() => falseShard.Contains(A<AttributeValue>.That.IsEqualTo(AttributeValue(true)))).Returns(false);
            A.CallTo(() => falseShard.Contains(A<AttributeValue>.That.IsEqualTo(AttributeValue(false)))).Returns(true);
        }

        [TestCase(new[] {true})]
        [TestCase(new[] {false})]
        [TestCase(new[] {true, true})]
        [TestCase(new[] {true, false})]
        [TestCase(new[] {false, true})]
        [TestCase(new[] {false, false})]
        public void Contains_NoShards(bool[] permanentAttributeValues)
        {
            var sut = new IndexShardMeta(
                ShardsByAttributeKey: new Dictionary<string, IIndexAttributeValueShard>(),
                DataSourceShardsToConsume: Array.Empty<DataSourceShardSubscription>());

            sut.Contains(PermanentAttributes(permanentAttributeValues)).Should().BeTrue();
        }

        [TestCase(new[] {true}, ExpectedResult = true)]
        [TestCase(new[] {false}, ExpectedResult = false)]
        [TestCase(new[] {true, true}, ExpectedResult = true)]
        [TestCase(new[] {true, false}, ExpectedResult = true)]
        [TestCase(new[] {false, true}, ExpectedResult = false)]
        [TestCase(new[] {false, false}, ExpectedResult = false)]
        public bool Contains_SingleShard_TrueShard(bool[] permanentAttributeValues)
        {
            var sut = new IndexShardMeta(
                ShardsByAttributeKey: new() {["k1"] = trueShard},
                DataSourceShardsToConsume: Array.Empty<DataSourceShardSubscription>());

            return sut.Contains(PermanentAttributes(permanentAttributeValues));
        }

        [TestCase(new[] {true}, ExpectedResult = false)]
        [TestCase(new[] {false}, ExpectedResult = true)]
        [TestCase(new[] {true, true}, ExpectedResult = false)]
        [TestCase(new[] {true, false}, ExpectedResult = false)]
        [TestCase(new[] {false, true}, ExpectedResult = true)]
        [TestCase(new[] {false, false}, ExpectedResult = true)]
        public bool Contains_SingleShard_FalseShard(bool[] permanentAttributeValues)
        {
            var sut = new IndexShardMeta(
                ShardsByAttributeKey: new() {["k1"] = falseShard},
                DataSourceShardsToConsume: Array.Empty<DataSourceShardSubscription>());

            return sut.Contains(PermanentAttributes(permanentAttributeValues));
        }

        [TestCase(new[] {true, true}, ExpectedResult = false)]
        [TestCase(new[] {true, false}, ExpectedResult = true)]
        [TestCase(new[] {false, true}, ExpectedResult = false)]
        [TestCase(new[] {false, false}, ExpectedResult = false)]
        public bool Contains_TwoShards(bool[] permanentAttributeValues)
        {
            var sut = new IndexShardMeta(
                ShardsByAttributeKey: new() {["k1"] = trueShard, ["k2"] = falseShard},
                DataSourceShardsToConsume: Array.Empty<DataSourceShardSubscription>());

            return sut.Contains(PermanentAttributes(permanentAttributeValues));
        }

        private static Dictionary<string, AttributeValue> PermanentAttributes(params bool[] permanentAttributeValues)
        {
            return permanentAttributeValues
                .Select((value, index) => (Key: $"k{index + 1}", Value: AttributeValue(value)))
                .ToDictionary(t => t.Key, t => t.Value);
        }
    }
}
