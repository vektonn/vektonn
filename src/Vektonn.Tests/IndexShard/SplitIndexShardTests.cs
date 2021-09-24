using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Vektonn.Contracts;
using Vektonn.Contracts.Sharding.DataSource;
using Vektonn.Contracts.Sharding.Index;
using Vektonn.Index;
using Vektonn.IndexShard;
using Vostok.Logging.Abstractions;
using static Vektonn.Tests.IndexShard.AttributeValueTestHelpers;

namespace Vektonn.Tests.IndexShard
{
    public class SplitIndexShardTests
    {
        private const int VectorDimension = 3;

        private SplitIndexShard<SparseVector> splitIndexShard = null!;

        [SetUp]
        public void SetUp()
        {
            var indexMeta = IndexMeta(
                indexIdAttributes: new[] {("Id", AttributeValueTypeCode.Int64)},
                splitAttributes: new[] {("SplitA", AttributeValueTypeCode.Bool), ("SplitZ", AttributeValueTypeCode.Bool)},
                indexPayloadAttributes: new[] {("Data", AttributeValueTypeCode.String)});

            var log = new SilentLog();
            splitIndexShard = new SplitIndexShard<SparseVector>(log, indexMeta, new IndexStoreFactory<byte[], byte[]>(log));

            splitIndexShard.UpdateIndex(
                new[]
                {
                    new DataPointOrTombstone<SparseVector>(
                        new DataPoint<SparseVector>(
                            SparseVector(y: 0),
                            Attributes: new Dictionary<string, AttributeValue>
                            {
                                {"Id", AttributeValue(0)},
                                {"SplitA", AttributeValue(false)},
                                {"SplitZ", AttributeValue(false)},
                                {"Data", AttributeValue("payload0")},
                            })),
                    new DataPointOrTombstone<SparseVector>(
                        new DataPoint<SparseVector>(
                            SparseVector(y: 1),
                            Attributes: new Dictionary<string, AttributeValue>
                            {
                                {"Id", AttributeValue(1)},
                                {"SplitA", AttributeValue(true)},
                                {"SplitZ", AttributeValue(true)},
                                {"Data", AttributeValue("payload1_splitTrue")},
                            })),
                    new DataPointOrTombstone<SparseVector>(
                        new DataPoint<SparseVector>(
                            SparseVector(y: -1),
                            Attributes: new Dictionary<string, AttributeValue>
                            {
                                {"Id", AttributeValue(2)},
                                {"SplitA", AttributeValue(true)},
                                {"SplitZ", AttributeValue(true)},
                                {"Data", AttributeValue("payload-1_splitTrue")},
                            })),
                    new DataPointOrTombstone<SparseVector>(
                        new DataPoint<SparseVector>(
                            SparseVector(y: 10),
                            Attributes: new Dictionary<string, AttributeValue>
                            {
                                {"Id", AttributeValue(3)},
                                {"SplitA", AttributeValue(true)},
                                {"SplitZ", AttributeValue(true)},
                                {"Data", AttributeValue("payload10_splitTrue")},
                            })),
                    new DataPointOrTombstone<SparseVector>(
                        new DataPoint<SparseVector>(
                            SparseVector(y: 1),
                            Attributes: new Dictionary<string, AttributeValue>
                            {
                                {"Id", AttributeValue(4)},
                                {"SplitA", AttributeValue(true)},
                                {"SplitZ", AttributeValue(false)},
                                {"Data", AttributeValue("payload1_splitFalse")},
                            })),
                    new DataPointOrTombstone<SparseVector>(
                        new DataPoint<SparseVector>(
                            SparseVector(y: -1),
                            Attributes: new Dictionary<string, AttributeValue>
                            {
                                {"Id", AttributeValue(5)},
                                {"SplitA", AttributeValue(true)},
                                {"SplitZ", AttributeValue(false)},
                                {"Data", AttributeValue("payload-1_splitFalse")},
                            })),
                    new DataPointOrTombstone<SparseVector>(
                        new DataPoint<SparseVector>(
                            SparseVector(y: -10),
                            Attributes: new Dictionary<string, AttributeValue>
                            {
                                {"Id", AttributeValue(6)},
                                {"SplitA", AttributeValue(true)},
                                {"SplitZ", AttributeValue(false)},
                                {"Data", AttributeValue("payload-10_splitFalse")},
                            })),
                });
        }

        [TearDown]
        public void TearDown()
        {
            splitIndexShard.Dispose();
        }

        [Test]
        public void Search_Update_And_Search()
        {
            var queryVector = SparseVector(y: 0.5);

            var searchQuery = new SearchQuery<SparseVector>(
                SplitFilter: new Dictionary<string, AttributeValue>
                {
                    {"SplitA", AttributeValue(true)},
                    {"SplitZ", AttributeValue(false)},
                },
                QueryVectors: new[] {queryVector},
                K: 2);

            splitIndexShard
                .FindNearest(searchQuery)
                .Should()
                .BeEquivalentTo(
                    new[]
                    {
                        new SearchResultItem<SparseVector>(
                            QueryVector: queryVector,
                            NearestDataPoints: new[]
                            {
                                new FoundDataPoint<SparseVector>(
                                    Vector: SparseVector(y: 1),
                                    new Dictionary<string, AttributeValue>
                                    {
                                        {"Id", AttributeValue(4)},
                                        {"SplitA", AttributeValue(true)},
                                        {"SplitZ", AttributeValue(false)},
                                        {"Data", AttributeValue("payload1_splitFalse")},
                                    },
                                    Distance: 0),
                                new FoundDataPoint<SparseVector>(
                                    Vector: SparseVector(y: -1),
                                    new Dictionary<string, AttributeValue>
                                    {
                                        {"Id", AttributeValue(5)},
                                        {"SplitA", AttributeValue(true)},
                                        {"SplitZ", AttributeValue(false)},
                                        {"Data", AttributeValue("payload-1_splitFalse")},
                                    },
                                    Distance: 2),
                            }),
                    },
                    o => o.WithStrictOrdering()
                );

            splitIndexShard.UpdateIndex(
                new[]
                {
                    new DataPointOrTombstone<SparseVector>(
                        new Tombstone(
                            IdAttributes: new Dictionary<string, AttributeValue>
                            {
                                {"Id", AttributeValue(5)},
                                {"SplitA", AttributeValue(true)},
                                {"SplitZ", AttributeValue(false)},
                            })),
                });

            splitIndexShard
                .FindNearest(searchQuery)
                .Should()
                .BeEquivalentTo(
                    new[]
                    {
                        new SearchResultItem<SparseVector>(
                            QueryVector: queryVector,
                            NearestDataPoints: new[]
                            {
                                new FoundDataPoint<SparseVector>(
                                    Vector: SparseVector(y: 1),
                                    new Dictionary<string, AttributeValue>
                                    {
                                        {"Id", AttributeValue(4)},
                                        {"SplitA", AttributeValue(true)},
                                        {"SplitZ", AttributeValue(false)},
                                        {"Data", AttributeValue("payload1_splitFalse")},
                                    },
                                    Distance: 0),
                                new FoundDataPoint<SparseVector>(
                                    Vector: SparseVector(y: -10),
                                    new Dictionary<string, AttributeValue>
                                    {
                                        {"Id", AttributeValue(6)},
                                        {"SplitA", AttributeValue(true)},
                                        {"SplitZ", AttributeValue(false)},
                                        {"Data", AttributeValue("payload-10_splitFalse")},
                                    },
                                    Distance: 2),
                            }),
                    },
                    o => o.WithStrictOrdering()
                );
        }

        [Test]
        public void Search_WithNoSplitFilter()
        {
            var queryVector = SparseVector(y: 0.5);

            splitIndexShard
                .FindNearest(
                    new SearchQuery<SparseVector>(
                        SplitFilter: new Dictionary<string, AttributeValue>(),
                        QueryVectors: new[] {queryVector},
                        K: 4))
                .Should()
                .BeEquivalentTo(
                    new[]
                    {
                        new SearchResultItem<SparseVector>(
                            QueryVector: queryVector,
                            NearestDataPoints: new[]
                            {
                                new FoundDataPoint<SparseVector>(
                                    Vector: SparseVector(y: 1),
                                    new Dictionary<string, AttributeValue>
                                    {
                                        {"Id", AttributeValue(1)},
                                        {"SplitA", AttributeValue(true)},
                                        {"SplitZ", AttributeValue(true)},
                                        {"Data", AttributeValue("payload1_splitTrue")},
                                    },
                                    Distance: 0),
                                new FoundDataPoint<SparseVector>(
                                    Vector: SparseVector(y: 10),
                                    new Dictionary<string, AttributeValue>
                                    {
                                        {"Id", AttributeValue(3)},
                                        {"SplitA", AttributeValue(true)},
                                        {"SplitZ", AttributeValue(true)},
                                        {"Data", AttributeValue("payload10_splitTrue")},
                                    },
                                    Distance: 0),
                                new FoundDataPoint<SparseVector>(
                                    Vector: SparseVector(y: 1),
                                    new Dictionary<string, AttributeValue>
                                    {
                                        {"Id", AttributeValue(4)},
                                        {"SplitA", AttributeValue(true)},
                                        {"SplitZ", AttributeValue(false)},
                                        {"Data", AttributeValue("payload1_splitFalse")},
                                    },
                                    Distance: 0),
                                new FoundDataPoint<SparseVector>(
                                    Vector: SparseVector(y: 0),
                                    new Dictionary<string, AttributeValue>
                                    {
                                        {"Id", AttributeValue(0)},
                                        {"SplitA", AttributeValue(false)},
                                        {"SplitZ", AttributeValue(false)},
                                        {"Data", AttributeValue("payload0")},
                                    },
                                    Distance: 1),
                            }),
                    },
                    o => o.WithStrictOrdering()
                );
        }

        [Test]
        public void Search_WithPartialSplitFilter()
        {
            var queryVector = SparseVector(y: 0.5);

            splitIndexShard
                .FindNearest(
                    new SearchQuery<SparseVector>(
                        SplitFilter: new Dictionary<string, AttributeValue>
                        {
                            {"SplitA", AttributeValue(true)},
                        },
                        QueryVectors: new[] {queryVector},
                        K: 3))
                .Should()
                .BeEquivalentTo(
                    new[]
                    {
                        new SearchResultItem<SparseVector>(
                            QueryVector: queryVector,
                            NearestDataPoints: new[]
                            {
                                new FoundDataPoint<SparseVector>(
                                    Vector: SparseVector(y: 1),
                                    new Dictionary<string, AttributeValue>
                                    {
                                        {"Id", AttributeValue(1)},
                                        {"SplitA", AttributeValue(true)},
                                        {"SplitZ", AttributeValue(true)},
                                        {"Data", AttributeValue("payload1_splitTrue")},
                                    },
                                    Distance: 0),
                                new FoundDataPoint<SparseVector>(
                                    Vector: SparseVector(y: 10),
                                    new Dictionary<string, AttributeValue>
                                    {
                                        {"Id", AttributeValue(3)},
                                        {"SplitA", AttributeValue(true)},
                                        {"SplitZ", AttributeValue(true)},
                                        {"Data", AttributeValue("payload10_splitTrue")},
                                    },
                                    Distance: 0),
                                new FoundDataPoint<SparseVector>(
                                    Vector: SparseVector(y: 1),
                                    new Dictionary<string, AttributeValue>
                                    {
                                        {"Id", AttributeValue(4)},
                                        {"SplitA", AttributeValue(true)},
                                        {"SplitZ", AttributeValue(false)},
                                        {"Data", AttributeValue("payload1_splitFalse")},
                                    },
                                    Distance: 0),
                            }),
                    },
                    o => o.WithStrictOrdering()
                );

            splitIndexShard
                .FindNearest(
                    new SearchQuery<SparseVector>(
                        SplitFilter: new Dictionary<string, AttributeValue>
                        {
                            {"SplitA", AttributeValue(false)},
                        },
                        QueryVectors: new[] {queryVector},
                        K: 3))
                .Should()
                .BeEquivalentTo(
                    new[]
                    {
                        new SearchResultItem<SparseVector>(
                            QueryVector: queryVector,
                            NearestDataPoints: new[]
                            {
                                new FoundDataPoint<SparseVector>(
                                    Vector: SparseVector(y: 0),
                                    new Dictionary<string, AttributeValue>
                                    {
                                        {"Id", AttributeValue(0)},
                                        {"SplitA", AttributeValue(false)},
                                        {"SplitZ", AttributeValue(false)},
                                        {"Data", AttributeValue("payload0")},
                                    },
                                    Distance: 1),
                            }),
                    },
                    o => o.WithStrictOrdering()
                );

            splitIndexShard
                .FindNearest(
                    new SearchQuery<SparseVector>(
                        SplitFilter: new Dictionary<string, AttributeValue>
                        {
                            {"SplitZ", AttributeValue(true)},
                        },
                        QueryVectors: new[] {queryVector},
                        K: 3))
                .Should()
                .BeEquivalentTo(
                    new[]
                    {
                        new SearchResultItem<SparseVector>(
                            QueryVector: queryVector,
                            NearestDataPoints: new[]
                            {
                                new FoundDataPoint<SparseVector>(
                                    Vector: SparseVector(y: 1),
                                    new Dictionary<string, AttributeValue>
                                    {
                                        {"Id", AttributeValue(1)},
                                        {"SplitA", AttributeValue(true)},
                                        {"SplitZ", AttributeValue(true)},
                                        {"Data", AttributeValue("payload1_splitTrue")},
                                    },
                                    Distance: 0),
                                new FoundDataPoint<SparseVector>(
                                    Vector: SparseVector(y: 10),
                                    new Dictionary<string, AttributeValue>
                                    {
                                        {"Id", AttributeValue(3)},
                                        {"SplitA", AttributeValue(true)},
                                        {"SplitZ", AttributeValue(true)},
                                        {"Data", AttributeValue("payload10_splitTrue")},
                                    },
                                    Distance: 0),
                                new FoundDataPoint<SparseVector>(
                                    Vector: SparseVector(y: -1),
                                    new Dictionary<string, AttributeValue>
                                    {
                                        {"Id", AttributeValue(2)},
                                        {"SplitA", AttributeValue(true)},
                                        {"SplitZ", AttributeValue(true)},
                                        {"Data", AttributeValue("payload-1_splitTrue")},
                                    },
                                    Distance: 2),
                            }),
                    },
                    o => o.WithStrictOrdering()
                );

            splitIndexShard
                .FindNearest(
                    new SearchQuery<SparseVector>(
                        SplitFilter: new Dictionary<string, AttributeValue>
                        {
                            {"SplitZ", AttributeValue(false)},
                        },
                        QueryVectors: new[] {queryVector},
                        K: 3))
                .Should()
                .BeEquivalentTo(
                    new[]
                    {
                        new SearchResultItem<SparseVector>(
                            QueryVector: queryVector,
                            NearestDataPoints: new[]
                            {
                                new FoundDataPoint<SparseVector>(
                                    Vector: SparseVector(y: 1),
                                    new Dictionary<string, AttributeValue>
                                    {
                                        {"Id", AttributeValue(4)},
                                        {"SplitA", AttributeValue(true)},
                                        {"SplitZ", AttributeValue(false)},
                                        {"Data", AttributeValue("payload1_splitFalse")},
                                    },
                                    Distance: 0),
                                new FoundDataPoint<SparseVector>(
                                    Vector: SparseVector(y: 0),
                                    new Dictionary<string, AttributeValue>
                                    {
                                        {"Id", AttributeValue(0)},
                                        {"SplitA", AttributeValue(false)},
                                        {"SplitZ", AttributeValue(false)},
                                        {"Data", AttributeValue("payload0")},
                                    },
                                    Distance: 1),
                                new FoundDataPoint<SparseVector>(
                                    Vector: SparseVector(y: -1),
                                    new Dictionary<string, AttributeValue>
                                    {
                                        {"Id", AttributeValue(5)},
                                        {"SplitA", AttributeValue(true)},
                                        {"SplitZ", AttributeValue(false)},
                                        {"Data", AttributeValue("payload-1_splitFalse")},
                                    },
                                    Distance: 2),
                            }),
                    },
                    o => o.WithStrictOrdering()
                );
        }

        [Test]
        public void Search_WithPartialSplitFilter_MatchingNoSplits()
        {
            var queryVector = SparseVector(y: 0.5);

            var searchQuery = new SearchQuery<SparseVector>(
                SplitFilter: new Dictionary<string, AttributeValue>
                {
                    {"SplitA", AttributeValue(false)},
                },
                QueryVectors: new[] {queryVector},
                K: 1);

            splitIndexShard
                .FindNearest(searchQuery)
                .Should()
                .BeEquivalentTo(
                    new[]
                    {
                        new SearchResultItem<SparseVector>(
                            QueryVector: queryVector,
                            NearestDataPoints: new[]
                            {
                                new FoundDataPoint<SparseVector>(
                                    Vector: SparseVector(y: 0),
                                    new Dictionary<string, AttributeValue>
                                    {
                                        {"Id", AttributeValue(0)},
                                        {"SplitA", AttributeValue(false)},
                                        {"SplitZ", AttributeValue(false)},
                                        {"Data", AttributeValue("payload0")},
                                    },
                                    Distance: 1),
                            }
                        ),
                    },
                    o => o.WithStrictOrdering()
                );

            splitIndexShard.UpdateIndex(
                new[]
                {
                    new DataPointOrTombstone<SparseVector>(
                        new Tombstone(
                            IdAttributes: new Dictionary<string, AttributeValue>
                            {
                                {"Id", AttributeValue(0)},
                                {"SplitA", AttributeValue(false)},
                                {"SplitZ", AttributeValue(false)},
                            })),
                });

            splitIndexShard
                .FindNearest(searchQuery)
                .Should()
                .BeEquivalentTo(
                    new[]
                    {
                        new SearchResultItem<SparseVector>(
                            QueryVector: queryVector,
                            NearestDataPoints: Array.Empty<FoundDataPoint<SparseVector>>()
                        ),
                    },
                    o => o.WithStrictOrdering()
                );
        }

        [Test]
        public void Update_WithSplitAttributeNotInIdAttributes()
        {
            var queryVector = SparseVector(y: 0.5);

            var searchQuery = new SearchQuery<SparseVector>(
                SplitFilter: new Dictionary<string, AttributeValue>
                {
                    {"SplitA", AttributeValue(false)},
                    {"SplitZ", AttributeValue(false)},
                },
                QueryVectors: new[] {queryVector},
                K: 1);

            splitIndexShard
                .FindNearest(searchQuery)
                .Should()
                .BeEquivalentTo(
                    new[]
                    {
                        new SearchResultItem<SparseVector>(
                            QueryVector: queryVector,
                            NearestDataPoints: new[]
                            {
                                new FoundDataPoint<SparseVector>(
                                    Vector: SparseVector(y: 0),
                                    new Dictionary<string, AttributeValue>
                                    {
                                        {"Id", AttributeValue(0)},
                                        {"SplitA", AttributeValue(false)},
                                        {"SplitZ", AttributeValue(false)},
                                        {"Data", AttributeValue("payload0")},
                                    },
                                    Distance: 1),
                            }
                        ),
                    },
                    o => o.WithStrictOrdering()
                );

            splitIndexShard.UpdateIndex(
                new[]
                {
                    new DataPointOrTombstone<SparseVector>(
                        new Tombstone(
                            IdAttributes: new Dictionary<string, AttributeValue>
                            {
                                {"Id", AttributeValue(0)},
                            })),
                });

            splitIndexShard
                .FindNearest(searchQuery)
                .Should()
                .BeEquivalentTo(
                    new[]
                    {
                        new SearchResultItem<SparseVector>(
                            QueryVector: queryVector,
                            NearestDataPoints: Array.Empty<FoundDataPoint<SparseVector>>()
                        ),
                    },
                    o => o.WithStrictOrdering()
                );
        }

        private static SparseVector SparseVector(double y)
        {
            return new SparseVector(VectorDimension, coordinates: new[] {y}, coordinateIndices: new[] {1});
        }

        private static IndexMeta IndexMeta(
            (string Key, AttributeValueTypeCode Type)[] indexIdAttributes,
            (string Key, AttributeValueTypeCode Type)[] splitAttributes,
            (string Key, AttributeValueTypeCode Type)[] indexPayloadAttributes)
        {
            return new IndexMeta(
                new DataSourceMeta(
                    VectorDimension,
                    VectorsAreSparse: true,
                    IdAttributes: indexIdAttributes.Select(t => t.Key).ToHashSet(),
                    DataSourceShardingMeta: new DataSourceShardingMeta(new Dictionary<string, IDataSourceAttributeValueSharder>()),
                    AttributeValueTypes: indexIdAttributes.Concat(splitAttributes).Concat(indexPayloadAttributes).ToDictionary(t => t.Key, t => t.Type)),
                IndexAlgorithm: Algorithms.SparnnIndexCosine,
                SplitAttributes: splitAttributes.Select(t => t.Key).ToHashSet(),
                IndexShardsMap: new IndexShardsMapMeta(new Dictionary<string, IndexShardMeta>()));
        }
    }
}
