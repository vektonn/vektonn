using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Vektonn.Contracts;
using Vektonn.Contracts.Sharding.DataSource;
using Vektonn.Contracts.Sharding.Index;
using Vektonn.Index;
using Vektonn.IndexShard;
using Vektonn.SharedImpl.BinarySerialization;
using static Vektonn.Tests.IndexShard.AttributeValueTestHelpers;

namespace Vektonn.Tests.IndexShard
{
    public class IndexDataPointSerializerTests
    {
        private const int VectorDimension = 3;

        private readonly AttributesAccessor attributesAccessor;

        public IndexDataPointSerializerTests()
        {
            var indexMeta = IndexMeta(
                indexIdAttributes: new[] {("IdA", AttributeValueTypeCode.Int64), ("IdZ", AttributeValueTypeCode.Int64)},
                splitAttributes: new[] {("SplitA", AttributeValueTypeCode.Bool), ("SplitZ", AttributeValueTypeCode.Bool)},
                indexPayloadAttributes: new[] {("DataA", AttributeValueTypeCode.String), ("DataZ", AttributeValueTypeCode.String)});

            attributesAccessor = new AttributesAccessor(indexMeta);
        }

        [Test]
        public void ToIndexDataPointOrTombstones()
        {
            var randomVector = RandomVector();
            new[]
                {
                    new DataPointOrTombstone<DenseVector>(
                        new Tombstone(
                            new Dictionary<string, AttributeValue>
                            {
                                {"IdA", AttributeValue(3)},
                                {"IdZ", AttributeValue(14)},
                                {"DataA", AttributeValue("payload1")},
                                {"DataZ", AttributeValue("payload2")},
                            })),
                    new DataPointOrTombstone<DenseVector>(
                        new DataPoint<DenseVector>(
                            randomVector,
                            new Dictionary<string, AttributeValue>
                            {
                                {"IdA", AttributeValue(15)},
                                {"IdZ", AttributeValue(92)},
                                {"DataA", AttributeValue("payload3")},
                                {"DataZ", AttributeValue("payload4")},
                            })),
                }.ToIndexDataPointOrTombstones(attributesAccessor)
                .Should()
                .BeEquivalentTo(
                    new[]
                    {
                        new IndexDataPointOrTombstone<byte[], byte[], DenseVector>(
                            new IndexTombstone<byte[]>(
                                Id: AttributeValueSerializer.Serialize(new[] {AttributeValue(3), AttributeValue(14)}))),
                        new IndexDataPointOrTombstone<byte[], byte[], DenseVector>(
                            new IndexDataPoint<byte[], byte[], DenseVector>(
                                Id: AttributeValueSerializer.Serialize(new[] {AttributeValue(15), AttributeValue(92)}),
                                Data: AttributeValueSerializer.Serialize(new[] {AttributeValue("payload3"), AttributeValue("payload4")}),
                                Vector: randomVector)),
                    },
                    o => o.WithStrictOrdering()
                );
        }

        [Test]
        public void ToSearchResults()
        {
            var queryVector1 = RandomVector();
            var nearestVector11 = RandomVector();
            var nearestVector12 = RandomVector();
            var queryVector2 = RandomVector();
            var nearestVector21 = RandomVector();
            var nearestVector22 = RandomVector();
            new[]
                {
                    new IndexSearchResultItem<byte[], byte[], DenseVector>(
                        QueryVector: queryVector1,
                        NearestDataPoints: new[]
                        {
                            new IndexFoundDataPoint<byte[], byte[], DenseVector>(
                                Id: AttributeValueSerializer.Serialize(new[] {AttributeValue(3), AttributeValue(14)}),
                                Data: AttributeValueSerializer.Serialize(new[] {AttributeValue("payload11"), AttributeValue("payload12")}),
                                Vector: nearestVector11,
                                Distance: 0.11
                            ),
                            new IndexFoundDataPoint<byte[], byte[], DenseVector>(
                                Id: AttributeValueSerializer.Serialize(new[] {AttributeValue(15), AttributeValue(92)}),
                                Data: AttributeValueSerializer.Serialize(new[] {AttributeValue("payload13"), AttributeValue("payload14")}),
                                Vector: nearestVector12,
                                Distance: 0.12
                            ),
                        }
                    ),
                    new IndexSearchResultItem<byte[], byte[], DenseVector>(
                        QueryVector: queryVector2,
                        NearestDataPoints: new[]
                        {
                            new IndexFoundDataPoint<byte[], byte[], DenseVector>(
                                Id: AttributeValueSerializer.Serialize(new[] {AttributeValue(2), AttributeValue(71)}),
                                Data: AttributeValueSerializer.Serialize(new[] {AttributeValue("payload21"), AttributeValue("payload22")}),
                                Vector: nearestVector21,
                                Distance: 0.21
                            ),
                            new IndexFoundDataPoint<byte[], byte[], DenseVector>(
                                Id: AttributeValueSerializer.Serialize(new[] {AttributeValue(82), AttributeValue(81)}),
                                Data: AttributeValueSerializer.Serialize(new[] {AttributeValue("payload23"), AttributeValue("payload24")}),
                                Vector: nearestVector22,
                                Distance: 0.22
                            ),
                        }
                    ),
                }.ToSearchResults(
                    attributesAccessor,
                    splitKeyBytes: AttributeValueSerializer.Serialize(new[] {AttributeValue(true), AttributeValue(false)}))
                .Should()
                .BeEquivalentTo(
                    new[]
                    {
                        new SearchResultItem<DenseVector>(
                            QueryVector: queryVector1,
                            new[]
                            {
                                new FoundDataPoint<DenseVector>(
                                    Vector: nearestVector11,
                                    Attributes: new Dictionary<string, AttributeValue>
                                    {
                                        {"IdA", AttributeValue(3)},
                                        {"IdZ", AttributeValue(14)},
                                        {"SplitA", AttributeValue(true)},
                                        {"SplitZ", AttributeValue(false)},
                                        {"DataA", AttributeValue("payload11")},
                                        {"DataZ", AttributeValue("payload12")},
                                    },
                                    Distance: 0.11),
                                new FoundDataPoint<DenseVector>(
                                    Vector: nearestVector12,
                                    Attributes: new Dictionary<string, AttributeValue>
                                    {
                                        {"IdA", AttributeValue(15)},
                                        {"IdZ", AttributeValue(92)},
                                        {"SplitA", AttributeValue(true)},
                                        {"SplitZ", AttributeValue(false)},
                                        {"DataA", AttributeValue("payload13")},
                                        {"DataZ", AttributeValue("payload14")},
                                    },
                                    Distance: 0.12),
                            }),
                        new SearchResultItem<DenseVector>(
                            QueryVector: queryVector2,
                            new[]
                            {
                                new FoundDataPoint<DenseVector>(
                                    Vector: nearestVector21,
                                    Attributes: new Dictionary<string, AttributeValue>
                                    {
                                        {"IdA", AttributeValue(2)},
                                        {"IdZ", AttributeValue(71)},
                                        {"SplitA", AttributeValue(true)},
                                        {"SplitZ", AttributeValue(false)},
                                        {"DataA", AttributeValue("payload21")},
                                        {"DataZ", AttributeValue("payload22")},
                                    },
                                    Distance: 0.21),
                                new FoundDataPoint<DenseVector>(
                                    Vector: nearestVector22,
                                    Attributes: new Dictionary<string, AttributeValue>
                                    {
                                        {"IdA", AttributeValue(82)},
                                        {"IdZ", AttributeValue(81)},
                                        {"SplitA", AttributeValue(true)},
                                        {"SplitZ", AttributeValue(false)},
                                        {"DataA", AttributeValue("payload23")},
                                        {"DataZ", AttributeValue("payload24")},
                                    },
                                    Distance: 0.22),
                            }),
                    },
                    o => o.WithStrictOrdering()
                );
        }

        private static DenseVector RandomVector()
        {
            return RandomHelpers.NextDenseVector(VectorDimension);
        }

        private static IndexMeta IndexMeta(
            (string Key, AttributeValueTypeCode Type)[] indexIdAttributes,
            (string Key, AttributeValueTypeCode Type)[] splitAttributes,
            (string Key, AttributeValueTypeCode Type)[] indexPayloadAttributes)
        {
            return new IndexMeta(
                new DataSourceMeta(
                    VectorDimension,
                    VectorsAreSparse: false,
                    IdAttributes: indexIdAttributes.Select(t => t.Key).ToHashSet(),
                    DataSourceShardingMeta: new DataSourceShardingMeta(new Dictionary<string, IDataSourceAttributeValueSharder>()),
                    AttributeValueTypes: indexIdAttributes.Concat(splitAttributes).Concat(indexPayloadAttributes).ToDictionary(t => t.Key, t => t.Type)),
                IndexAlgorithm: Algorithms.FaissIndexFlatL2,
                SplitAttributes: splitAttributes.Select(t => t.Key).ToHashSet(),
                IndexShardsMap: new IndexShardsMapMeta(new Dictionary<string, IndexShardMeta>()));
        }
    }
}
