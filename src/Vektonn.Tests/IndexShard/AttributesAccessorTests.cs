using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Vektonn.Index;
using Vektonn.IndexShard;
using Vektonn.SharedImpl.Contracts;
using Vektonn.SharedImpl.Contracts.Sharding.DataSource;
using Vektonn.SharedImpl.Contracts.Sharding.Index;
using static Vektonn.Tests.IndexShard.AttributeValueTestHelpers;

namespace Vektonn.Tests.IndexShard
{
    public class AttributesAccessorTests
    {
        [Test]
        public void GetIndexId()
        {
            var sut = new AttributesAccessor(
                IndexMeta(
                    indexIdAttributes: new[] {("IdA", AttributeValueTypeCode.Int64), ("IdZ", AttributeValueTypeCode.String)},
                    splitAttributes: new[] {("Split", AttributeValueTypeCode.Bool)},
                    indexPayloadAttributes: new[] {("Data", AttributeValueTypeCode.String)}));

            sut.GetIndexId(
                    new Dictionary<string, AttributeValue>
                    {
                        {"IdZ", AttributeValue("id")},
                        {"IdA", AttributeValue(42)},
                        {"Split", AttributeValue(true)},
                        {"Data", AttributeValue("payload")}
                    })
                .Should()
                .BeEquivalentTo(new[] {AttributeValue(42), AttributeValue("id")}, o => o.WithStrictOrdering());
        }

        [Test]
        public void GetPartialSplitKey()
        {
            var sut = new AttributesAccessor(
                IndexMeta(
                    indexIdAttributes: new[] {("Id", AttributeValueTypeCode.Int64)},
                    splitAttributes: new[] {("SplitA", AttributeValueTypeCode.Bool), ("SplitZ", AttributeValueTypeCode.Int64)},
                    indexPayloadAttributes: new[] {("Data", AttributeValueTypeCode.String)}));

            sut.GetPartialSplitKey(attributes: null)
                .Should()
                .BeEquivalentTo(new AttributeValue?[] {null, null}, o => o.WithStrictOrdering());

            sut.GetPartialSplitKey(
                    new Dictionary<string, AttributeValue>
                    {
                        {"Id", AttributeValue("id")},
                        {"Data", AttributeValue("payload")}
                    })
                .Should()
                .BeEquivalentTo(new AttributeValue?[] {null, null}, o => o.WithStrictOrdering());

            sut.GetPartialSplitKey(
                    new Dictionary<string, AttributeValue>
                    {
                        {"Id", AttributeValue("id")},
                        {"SplitA", AttributeValue(true)},
                        {"Data", AttributeValue("payload")}
                    })
                .Should()
                .BeEquivalentTo(new[] {AttributeValue(true), null}, o => o.WithStrictOrdering());

            sut.GetPartialSplitKey(
                    new Dictionary<string, AttributeValue>
                    {
                        {"SplitZ", AttributeValue(1)},
                    })
                .Should()
                .BeEquivalentTo(new[] {null, AttributeValue(1)}, o => o.WithStrictOrdering());

            sut.GetPartialSplitKey(
                    new Dictionary<string, AttributeValue>
                    {
                        {"Id", AttributeValue("id")},
                        {"SplitA", AttributeValue(false)},
                        {"SplitZ", AttributeValue(42)},
                        {"Data", AttributeValue("payload")}
                    })
                .Should()
                .BeEquivalentTo(new[] {AttributeValue(false), AttributeValue(42)}, o => o.WithStrictOrdering());
        }

        [Test]
        public void GetSplitKey()
        {
            var sut = new AttributesAccessor(
                IndexMeta(
                    indexIdAttributes: new[] {("Id", AttributeValueTypeCode.Int64)},
                    splitAttributes: new[] {("SplitA", AttributeValueTypeCode.Bool), ("SplitZ", AttributeValueTypeCode.Int64)},
                    indexPayloadAttributes: new[] {("Data", AttributeValueTypeCode.String)}));

            sut.GetSplitKey(
                    new Dictionary<string, AttributeValue>
                    {
                        {"Id", AttributeValue("id")},
                        {"SplitA", AttributeValue(false)},
                        {"SplitZ", AttributeValue(42)},
                        {"Data", AttributeValue("payload")}
                    })
                .Should()
                .BeEquivalentTo(new[] {AttributeValue(false), AttributeValue(42)}, o => o.WithStrictOrdering());

            Action action = () => sut.GetSplitKey(
                new Dictionary<string, AttributeValue>
                {
                    {"Id", AttributeValue("id")},
                    {"SplitA", AttributeValue(true)},
                    {"Data", AttributeValue("payload")}
                });
            action.Should().Throw<InvalidOperationException>().WithMessage("attributes does not contain all splitAttributes");
        }

        [Test]
        public void TryGetPayload()
        {
            var sut = new AttributesAccessor(
                IndexMeta(
                    indexIdAttributes: new[] {("Id", AttributeValueTypeCode.Int64)},
                    splitAttributes: new[] {("Split", AttributeValueTypeCode.Bool)},
                    indexPayloadAttributes: new[] {("DataA", AttributeValueTypeCode.String), ("DataZ", AttributeValueTypeCode.Int64)}));

            sut.TryGetPayload(
                    new Dictionary<string, AttributeValue>
                    {
                        {"Id", AttributeValue("id")},
                        {"Split", AttributeValue(42)},
                        {"DataA", AttributeValue("payload1")},
                        {"DataZ", AttributeValue("payload2")},
                    })
                .Should()
                .BeEquivalentTo(new[] {AttributeValue("payload1"), AttributeValue("payload2")}, o => o.WithStrictOrdering());
        }

        [Test]
        public void TryGetPayload_NoPayloadAttributes()
        {
            var sut = new AttributesAccessor(
                IndexMeta(
                    indexIdAttributes: new[] {("Id", AttributeValueTypeCode.Int64)},
                    splitAttributes: new[] {("Split", AttributeValueTypeCode.Bool)},
                    indexPayloadAttributes: new (string Key, AttributeValueTypeCode Type)[] {}));

            sut.TryGetPayload(
                    new Dictionary<string, AttributeValue>
                    {
                        {"Id", AttributeValue("id")},
                        {"Split", AttributeValue(42)},
                    })
                .Should()
                .BeNull();

            sut.TryGetPayload(
                    new Dictionary<string, AttributeValue>
                    {
                        {"Id", AttributeValue("id")},
                        {"Split", AttributeValue(42)},
                        {"Data", AttributeValue("payload")},
                    })
                .Should()
                .BeNull();
        }

        [Test]
        public void GetAttributes()
        {
            var sut = new AttributesAccessor(
                IndexMeta(
                    indexIdAttributes: new[] {("IdA", AttributeValueTypeCode.Int64), ("IdZ", AttributeValueTypeCode.Int64)},
                    splitAttributes: new[] {("SplitA", AttributeValueTypeCode.Bool), ("SplitZ", AttributeValueTypeCode.Bool)},
                    indexPayloadAttributes: new[] {("DataA", AttributeValueTypeCode.String), ("DataZ", AttributeValueTypeCode.String)}));

            sut.GetAttributes(
                    indexId: new[] {AttributeValue(3), AttributeValue(14)},
                    splitKey: new[] {AttributeValue(true), AttributeValue(false)},
                    payload: new[] {AttributeValue("payload1"), AttributeValue("payload2")})
                .Should()
                .BeEquivalentTo(
                    new Dictionary<string, AttributeValue>
                    {
                        {"IdA", AttributeValue(3)},
                        {"IdZ", AttributeValue(14)},
                        {"SplitA", AttributeValue(true)},
                        {"SplitZ", AttributeValue(false)},
                        {"DataA", AttributeValue("payload1")},
                        {"DataZ", AttributeValue("payload2")},
                    });

            Action action = () => sut.GetAttributes(
                indexId: new[] {AttributeValue(3)},
                splitKey: new[] {AttributeValue(true), AttributeValue(false)},
                payload: new[] {AttributeValue("payload1"), AttributeValue("payload2")});
            action.Should().Throw<InvalidOperationException>().WithMessage("keys.Length (6) != values.Length (5)");
        }

        [Test]
        public void GetAttributes_NoSplitKey()
        {
            var sut = new AttributesAccessor(
                IndexMeta(
                    indexIdAttributes: new[] {("IdA", AttributeValueTypeCode.Int64), ("IdZ", AttributeValueTypeCode.Int64)},
                    splitAttributes: new (string Key, AttributeValueTypeCode Type)[] {},
                    indexPayloadAttributes: new[] {("DataA", AttributeValueTypeCode.String), ("DataZ", AttributeValueTypeCode.String)}));

            sut.GetAttributes(
                    indexId: new[] {AttributeValue(3), AttributeValue(14)},
                    splitKey: null,
                    payload: new[] {AttributeValue("payload1"), AttributeValue("payload2")})
                .Should()
                .BeEquivalentTo(
                    new Dictionary<string, AttributeValue>
                    {
                        {"IdA", AttributeValue(3)},
                        {"IdZ", AttributeValue(14)},
                        {"DataA", AttributeValue("payload1")},
                        {"DataZ", AttributeValue("payload2")},
                    });
        }

        [Test]
        public void GetAttributes_NoPayload()
        {
            var sut = new AttributesAccessor(
                IndexMeta(
                    indexIdAttributes: new[] {("IdA", AttributeValueTypeCode.Int64), ("IdZ", AttributeValueTypeCode.Int64)},
                    splitAttributes: new[] {("SplitA", AttributeValueTypeCode.Bool), ("SplitZ", AttributeValueTypeCode.Bool)},
                    indexPayloadAttributes: new (string Key, AttributeValueTypeCode Type)[] {}));

            sut.GetAttributes(
                    indexId: new[] {AttributeValue(3), AttributeValue(14)},
                    splitKey: new[] {AttributeValue(true), AttributeValue(false)},
                    payload: null)
                .Should()
                .BeEquivalentTo(
                    new Dictionary<string, AttributeValue>
                    {
                        {"IdA", AttributeValue(3)},
                        {"IdZ", AttributeValue(14)},
                        {"SplitA", AttributeValue(true)},
                        {"SplitZ", AttributeValue(false)},
                    });
        }

        private static IndexMeta IndexMeta(
            (string Key, AttributeValueTypeCode Type)[] indexIdAttributes,
            (string Key, AttributeValueTypeCode Type)[] splitAttributes,
            (string Key, AttributeValueTypeCode Type)[] indexPayloadAttributes)
        {
            return new IndexMeta(
                new IndexId(Name: "test", Version: "1.0"),
                new DataSourceMeta(
                    new DataSourceId(Name: "test", Version: "1.0"),
                    VectorDimension: 32,
                    VectorsAreSparse: false,
                    PermanentAttributes: indexIdAttributes.Select(t => t.Key).Concat(splitAttributes.Select(t => t.Key)).ToHashSet(),
                    DataSourceShardingMeta: new DataSourceShardingMeta(new Dictionary<string, IDataSourceAttributeValueSharder>()),
                    AttributeValueTypes: indexIdAttributes.Concat(splitAttributes).Concat(indexPayloadAttributes).ToDictionary(t => t.Key, t => t.Type)),
                IndexAlgorithm: Algorithms.FaissIndexFlatL2,
                IdAttributes: indexIdAttributes.Select(t => t.Key).ToHashSet(),
                SplitAttributes: splitAttributes.Select(t => t.Key).ToHashSet(),
                IndexShardsMap: new IndexShardsMapMeta(new Dictionary<string, IndexShardMeta>()));
        }
    }
}
