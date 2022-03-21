using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using Vektonn.SharedImpl.Contracts;
using Vektonn.SharedImpl.Contracts.Sharding.DataSource;
using static Vektonn.Tests.AttributeValueTestHelpers;

namespace Vektonn.Tests.SharedImpl.Sharding
{
    public class DataSourceShardingMetaTests
    {
        private IDataSourceAttributeValueSharder identitySharder = null!;
        private IDataSourceAttributeValueSharder twoBucketsSharder = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            identitySharder = A.Fake<IDataSourceAttributeValueSharder>();
            A.CallTo(() => identitySharder.GetShardingCoordinate(A<AttributeValue>.That.Matches(x => x.Int64 != null)))
                .ReturnsLazily((AttributeValue value) => (ulong)value.Int64!.Value);

            twoBucketsSharder = A.Fake<IDataSourceAttributeValueSharder>();
            A.CallTo(() => twoBucketsSharder.GetShardingCoordinate(A<AttributeValue>.That.Matches(x => x.Int64 != null)))
                .ReturnsLazily((AttributeValue value) => (ulong)value.Int64!.Value % 2);
        }

        [TestCase(new int[] {})]
        [TestCase(new[] {0})]
        [TestCase(new[] {1, 2})]
        public void GetDataSourceShardingCoordinates_NoShards(int[] attributeValues)
        {
            var sut = new DataSourceShardingMeta(
                ShardersByAttributeKey: new Dictionary<string, IDataSourceAttributeValueSharder>());

            sut.GetDataSourceShardingCoordinates(Attributes(attributeValues)).Should().BeEmpty();
        }

        [TestCase(new[] {0})]
        [TestCase(new[] {1, 2})]
        [TestCase(new[] {2, 1})]
        [TestCase(new[] {3, 1, 2})]
        public void GetDataSourceShardingCoordinates_SingleShard(int[] attributeValues)
        {
            var sut = new DataSourceShardingMeta(
                ShardersByAttributeKey: new() {["k1"] = identitySharder});

            sut.GetDataSourceShardingCoordinates(Attributes(attributeValues))
                .Should()
                .BeEquivalentTo(new Dictionary<string, ulong> {["k1"] = (ulong)attributeValues.First()});
        }

        [TestCase(new[] {1, 2})]
        [TestCase(new[] {2, 1})]
        [TestCase(new[] {3, 5, 2})]
        public void GetDataSourceShardingCoordinates_TwoShards(int[] attributeValues)
        {
            var sut = new DataSourceShardingMeta(
                ShardersByAttributeKey: new()
                {
                    ["k1"] = identitySharder,
                    ["k2"] = twoBucketsSharder,
                });

            sut.GetDataSourceShardingCoordinates(Attributes(attributeValues))
                .Should()
                .BeEquivalentTo(
                    new Dictionary<string, ulong>
                    {
                        ["k1"] = (ulong)attributeValues.First(),
                        ["k2"] = (ulong)attributeValues.Skip(1).First() % 2,
                    });
        }

        private static Dictionary<string, AttributeValue> Attributes(params int[] attributeValues)
        {
            return attributeValues
                .Select((value, index) => (Key: $"k{index + 1}", Value: AttributeValue(value)))
                .ToDictionary(t => t.Key, t => t.Value);
        }
    }
}
