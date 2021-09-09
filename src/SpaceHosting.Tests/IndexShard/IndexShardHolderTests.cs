using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SpaceHosting.Contracts;
using SpaceHosting.Contracts.ApiModels;
using SpaceHosting.Contracts.Sharding.DataSource;
using SpaceHosting.Contracts.Sharding.Index;
using SpaceHosting.Index;
using SpaceHosting.IndexShard;
using Vostok.Logging.Abstractions;
using static SpaceHosting.Tests.Contracts.ApiModels.AttributeDtoTestHelpers;
using static SpaceHosting.Tests.IndexShard.AttributeValueTestHelpers;

namespace SpaceHosting.Tests.IndexShard
{
    public class IndexShardHolderTests
    {
        private const int VectorDimension = 3;

        private IndexShardHolder<SparseVector> indexHolder = null!;

        [SetUp]
        public void SetUp()
        {
            var indexMeta = IndexMeta(
                indexIdAttributes: new[] {("IdA", AttributeValueTypeCode.Int64), ("IdZ", AttributeValueTypeCode.Bool)},
                indexPayloadAttributes: new[] {("Data", AttributeValueTypeCode.String)});

            indexHolder = new IndexShardHolder<SparseVector>(new SilentLog(), indexMeta);

            indexHolder.UpdateIndexShard(
                new[]
                {
                    new DataPointOrTombstone<SparseVector>(
                        new DataPoint<SparseVector>(
                            SparseVector(y: 1),
                            new Dictionary<string, AttributeValue>
                            {
                                {"IdA", AttributeValue(1)},
                                {"IdZ", AttributeValue(true)},
                                {"Data", AttributeValue("payload1")},
                            })),
                    new DataPointOrTombstone<SparseVector>(
                        new DataPoint<SparseVector>(
                            SparseVector(y: -1),
                            new Dictionary<string, AttributeValue>
                            {
                                {"IdA", AttributeValue(-1)},
                                {"IdZ", AttributeValue(false)},
                                {"Data", AttributeValue("payload-1")},
                            })),
                    new DataPointOrTombstone<SparseVector>(
                        new DataPoint<SparseVector>(
                            SparseVector(y: 10),
                            new Dictionary<string, AttributeValue>
                            {
                                {"IdA", AttributeValue(10)},
                                {"IdZ", AttributeValue(true)},
                                {"Data", AttributeValue("payload10")},
                            })),
                    new DataPointOrTombstone<SparseVector>(
                        new DataPoint<SparseVector>(
                            SparseVector(y: -10),
                            new Dictionary<string, AttributeValue>
                            {
                                {"IdA", AttributeValue(-10)},
                                {"IdZ", AttributeValue(false)},
                                {"Data", AttributeValue("payload-10")},
                            })),
                });
        }

        [TearDown]
        public void TearDown()
        {
            indexHolder.Dispose();
        }

        [Test]
        public void Search_Update_And_Search()
        {
            var queryVector1 = SparseVector(y: 0.5);
            var queryVector2 = SparseVector(y: -0.5);
            var searchQuery1 = new SearchQueryDto(SplitFilter: null, new[] {queryVector1.ToVectorDto(), queryVector2.ToVectorDto()}, K: 1);
            var searchResults1 = indexHolder.ExecuteSearchQuery(searchQuery1);

            searchResults1
                .Should()
                .BeEquivalentTo(
                    new[]
                    {
                        new SearchResultDto(
                            QueryVector: queryVector1.ToVectorDto(),
                            new[]
                            {
                                new FoundDataPointDto(
                                    Vector: SparseVector(y: 1).ToVectorDto(),
                                    Attributes: new[]
                                    {
                                        Attribute("IdA", value: 1),
                                        Attribute("IdZ", value: true),
                                        Attribute("Data", "payload1"),
                                    },
                                    Distance: 0),
                            }),
                        new SearchResultDto(
                            QueryVector: queryVector2.ToVectorDto(),
                            new[]
                            {
                                new FoundDataPointDto(
                                    Vector: SparseVector(y: -1).ToVectorDto(),
                                    Attributes: new[]
                                    {
                                        Attribute("IdA", value: -1),
                                        Attribute("IdZ", value: false),
                                        Attribute("Data", "payload-1"),
                                    },
                                    Distance: 0),
                            }),
                    },
                    o => o.RespectingRuntimeTypes().WithStrictOrdering()
                );

            indexHolder.UpdateIndexShard(
                new[]
                {
                    new DataPointOrTombstone<SparseVector>(
                        new Tombstone(
                            new Dictionary<string, AttributeValue>
                            {
                                {"IdA", AttributeValue(1)},
                                {"IdZ", AttributeValue(true)},
                            })),
                });

            var searchQuery2 = new SearchQueryDto(SplitFilter: null, new[] {queryVector1.ToVectorDto()}, K: 2);
            var searchResults2 = indexHolder.ExecuteSearchQuery(searchQuery2);

            searchResults2
                .Should()
                .BeEquivalentTo(
                    new[]
                    {
                        new SearchResultDto(
                            QueryVector: queryVector1.ToVectorDto(),
                            new[]
                            {
                                new FoundDataPointDto(
                                    Vector: SparseVector(y: 10).ToVectorDto(),
                                    Attributes: new[]
                                    {
                                        Attribute("IdA", value: 10),
                                        Attribute("IdZ", value: true),
                                        Attribute("Data", "payload10"),
                                    },
                                    Distance: 0),
                                new FoundDataPointDto(
                                    Vector: SparseVector(y: -1).ToVectorDto(),
                                    Attributes: new[]
                                    {
                                        Attribute("IdA", value: -1),
                                        Attribute("IdZ", value: false),
                                        Attribute("Data", "payload-1"),
                                    },
                                    Distance: 2),
                            }),
                    },
                    o => o.RespectingRuntimeTypes().WithStrictOrdering()
                );
        }

        private static SparseVector SparseVector(double y)
        {
            return new SparseVector(VectorDimension, coordinates: new[] {y}, coordinateIndices: new[] {1});
        }

        private static IndexMeta IndexMeta(
            (string Key, AttributeValueTypeCode Type)[] indexIdAttributes,
            (string Key, AttributeValueTypeCode Type)[] indexPayloadAttributes)
        {
            return new IndexMeta(
                new DataSourceMeta(
                    VectorDimension,
                    VectorsAreSparse: true,
                    IdAttributes: indexIdAttributes.Select(t => t.Key).ToHashSet(),
                    DataSourceShardingMeta: new DataSourceShardingMeta(new Dictionary<string, IDataSourceAttributeValueSharder>()),
                    AttributeValueTypes: indexIdAttributes.Concat(indexPayloadAttributes).ToDictionary(t => t.Key, t => t.Type)),
                IndexAlgorithm: Algorithms.SparnnIndexCosine,
                SplitAttributes: new HashSet<string>(),
                IndexShardsMap: new IndexShardsMapMeta(new Dictionary<string, IndexShardMeta>()));
        }
    }
}
