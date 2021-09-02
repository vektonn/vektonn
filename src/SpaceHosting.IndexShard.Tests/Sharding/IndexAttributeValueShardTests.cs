using FakeItEasy;
using MoreLinq;
using NUnit.Framework;
using SpaceHosting.IndexShard.Models;
using SpaceHosting.IndexShard.Models.Sharding;
using SpaceHosting.IndexShard.Models.Sharding.Index;

namespace SpaceHosting.IndexShard.Tests.Sharding
{
    public class IndexAttributeValueShardTests
    {
        private IAttributeValueProjector<int> projector = null!;

        [SetUp]
        public void SetUp()
        {
            projector = A.Fake<IAttributeValueProjector<int>>();
        }

        [TestCase(IndexShardingRule.BelongToSet, new int[] {}, ExpectedResult = false)]
        [TestCase(IndexShardingRule.BelongToSet, new[] {0, 1}, ExpectedResult = false)]
        [TestCase(IndexShardingRule.BelongToSet, new[] {42}, ExpectedResult = true)]
        [TestCase(IndexShardingRule.BelongToSet, new[] {0, 1, 42, 43}, ExpectedResult = true)]
        [TestCase(IndexShardingRule.BelongToComplementSet, new int[] {}, ExpectedResult = true)]
        [TestCase(IndexShardingRule.BelongToComplementSet, new[] {0, 1}, ExpectedResult = true)]
        [TestCase(IndexShardingRule.BelongToComplementSet, new[] {42}, ExpectedResult = false)]
        [TestCase(IndexShardingRule.BelongToComplementSet, new[] {0, 1, 42, 43}, ExpectedResult = false)]
        public bool Contains(IndexShardingRule indexShardingRule, int[] shardValues)
        {
            var sut = new IndexAttributeValueShard<int>(indexShardingRule, shardValues.ToHashSet(), projector);

            var attributeValue = new AttributeValue();
            A.CallTo(() => projector.GetProjection(attributeValue)).Returns(42);

            return sut.Contains(attributeValue);
        }
    }
}
