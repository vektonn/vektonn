using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using Vektonn.Contracts;
using Vektonn.Contracts.ApiModels;
using Vektonn.Contracts.Sharding.Index;

namespace Vektonn.Tests.Contracts.Sharding
{
    public class IndexShardsMapMetaTests
    {
        private IIndexAttributeValueShard trueShard = null!;
        private IIndexAttributeValueShard falseShard = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            trueShard = A.Fake<IIndexAttributeValueShard>();
            A.CallTo(() => trueShard.Contains(A<AttributeValue>.Ignored)).Returns(true);

            falseShard = A.Fake<IIndexAttributeValueShard>();
            A.CallTo(() => falseShard.Contains(A<AttributeValue>.Ignored)).Returns(false);
        }

        [TestCase(null)]
        [TestCase(arg: new string[0])]
        [TestCase(arg: new[] {"k1"})]
        [TestCase(arg: new[] {"k1", "k2"})]
        public void GetShardIdsForQuery_NoShards(string[]? splitFilterKeys)
        {
            var sut = new IndexShardsMapMeta(new Dictionary<string, IndexShardMeta>());

            sut.GetShardIdsForQuery(SplitFilter(splitFilterKeys)).Should().BeEmpty();
        }

        [TestCase(null)]
        [TestCase(arg: new string[0])]
        [TestCase(arg: new[] {"k1"})]
        [TestCase(arg: new[] {"k1", "k2"})]
        public void GetShardIdsForQuery_TwoShards_WithNoShardKeys(string[]? splitFilterKeys)
        {
            var sut = new IndexShardsMapMeta(
                new Dictionary<string, IndexShardMeta>
                {
                    {"0", new IndexShardMeta(new Dictionary<string, IIndexAttributeValueShard>())},
                    {"42-id", new IndexShardMeta(new Dictionary<string, IIndexAttributeValueShard>())},
                });

            sut.GetShardIdsForQuery(SplitFilter(splitFilterKeys)).Should().BeEquivalentTo("42-id", "0");
        }

        [TestCase(null, ExpectedResult = new[] {1, 2, 3})]
        [TestCase(arg: new[] {"k1"}, ExpectedResult = new[] {1, 3})]
        [TestCase(arg: new[] {"k2"}, ExpectedResult = new[] {1, 2, 3})]
        [TestCase(arg: new[] {"k1", "k2"}, ExpectedResult = new[] {1, 3})]
        [TestCase(arg: new[] {"k2", "k1"}, ExpectedResult = new[] {1, 3})]
        public int[] GetShardIdsForQuery_SingleShardKey(string[]? splitFilterKeys)
        {
            var sut = new IndexShardsMapMeta(
                new Dictionary<string, IndexShardMeta>
                {
                    {"1", ShardsByAttributeKey(("k1", trueShard))},
                    {"2", ShardsByAttributeKey(("k1", falseShard))},
                    {"3", ShardsByAttributeKey(("k1", trueShard))},
                });

            return sut.GetShardIdsForQuery(SplitFilter(splitFilterKeys)).Select(int.Parse).OrderBy(x => x).ToArray();
        }

        [TestCase(null, ExpectedResult = new[] {1, 2, 3, 5})]
        [TestCase(arg: new[] {"k1"}, ExpectedResult = new[] {1, 3})]
        [TestCase(arg: new[] {"k2"}, ExpectedResult = new[] {1, 2})]
        [TestCase(arg: new[] {"k1", "k2"}, ExpectedResult = new[] {1})]
        [TestCase(arg: new[] {"k1", "k3", "k2"}, ExpectedResult = new[] {1})]
        public int[] GetShardIdsForQuery_TwoShardKeys(string[]? splitFilterKeys)
        {
            var sut = new IndexShardsMapMeta(
                new Dictionary<string, IndexShardMeta>
                {
                    {"1", ShardsByAttributeKey(("k1", trueShard), ("k2", trueShard))},
                    {"2", ShardsByAttributeKey(("k1", falseShard), ("k2", trueShard))},
                    {"3", ShardsByAttributeKey(("k1", trueShard), ("k2", falseShard))},
                    {"5", ShardsByAttributeKey(("k1", falseShard), ("k2", falseShard))},
                });

            return sut.GetShardIdsForQuery(SplitFilter(splitFilterKeys)).Select(int.Parse).OrderBy(x => x).ToArray();
        }

        private static AttributeDto[]? SplitFilter(params string[]? splitFilterKeys)
        {
            return splitFilterKeys?.Select(key => new AttributeDto(key, new AttributeValue())).ToArray();
        }

        private static IndexShardMeta ShardsByAttributeKey(params (string AttributeKey, IIndexAttributeValueShard Shard)[] shards)
        {
            return new IndexShardMeta(shards.ToDictionary(t => t.AttributeKey, t => t.Shard));
        }
    }
}
