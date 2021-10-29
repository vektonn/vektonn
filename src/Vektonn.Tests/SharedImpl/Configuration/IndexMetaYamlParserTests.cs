using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Equivalency;
using MoreLinq;
using NUnit.Framework;
using Vektonn.Index;
using Vektonn.SharedImpl.Configuration;
using Vektonn.SharedImpl.Contracts;
using Vektonn.SharedImpl.Contracts.Sharding;
using Vektonn.SharedImpl.Contracts.Sharding.DataSource;
using Vektonn.SharedImpl.Contracts.Sharding.Index;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Vektonn.Tests.SharedImpl.Configuration
{
    public class IndexMetaYamlParserTests
    {
        private static readonly AttributeValueHasher AttributeValueHasher = new AttributeValueHasher();

        private readonly IndexMetaYamlParser sut = new(AttributeValueHasher);

        private readonly ISerializer yamlSerializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        [Test]
        public void ParseDataSourceId()
        {
            var dto = new IndexMetaYamlParser.DataSourceReferenceDto
            {
                DataSourceId = new IndexMetaYamlParser.IdDto
                {
                    Name = "Test.Name",
                    Version = "1.0"
                }
            };

            var yaml = yamlSerializer.Serialize(dto);
            yaml.Should()
                .Be(
                    @"dataSourceId:
  name: Test.Name
  version: 1.0
");

            sut.ParseDataSourceId(yaml)
                .Should()
                .BeEquivalentTo(new DataSourceId(Name: "Test.Name", Version: "1.0"));
        }

        [Test]
        public void ParseDataSourceMeta()
        {
            var dataSourceMetaDto = new IndexMetaYamlParser.DataSourceMetaDto
            {
                VectorDimension = 42,
                VectorsAreSparse = true,
                AttributeValueTypes = new Dictionary<string, AttributeValueTypeCode>
                {
                    {"Id", AttributeValueTypeCode.Int64},
                    {"ShardId1", AttributeValueTypeCode.Guid},
                    {"ShardId2", AttributeValueTypeCode.Bool},
                    {"Payload", AttributeValueTypeCode.String}
                },
                PermanentAttributes = new[] {"Id", "ShardId1", "ShardId2"},
                ShardersByAttributeKey = new Dictionary<string, IndexMetaYamlParser.DataSourceAttributeValueSharderDto>
                {
                    {"ShardId1", new IndexMetaYamlParser.DataSourceAttributeValueSharderDto {NumberOfShards = 7}},
                    {"ShardId2", new IndexMetaYamlParser.DataSourceAttributeValueSharderDto {NumberOfShards = null}},
                }
            };

            var dataSourceYaml = yamlSerializer.Serialize(dataSourceMetaDto);
            dataSourceYaml.Should()
                .Be(
                    @"vectorDimension: 42
vectorsAreSparse: true
attributeValueTypes:
  Id: Int64
  ShardId1: Guid
  ShardId2: Bool
  Payload: String
permanentAttributes:
- Id
- ShardId1
- ShardId2
shardersByAttributeKey:
  ShardId1:
    numberOfShards: 7
  ShardId2:
    numberOfShards: 
");

            var dataSourceId = new DataSourceId(Name: "Test.Name.Source", Version: "1.0");

            sut.ParseDataSourceMeta((dataSourceId, dataSourceYaml))
                .Should()
                .BeEquivalentTo(
                    new DataSourceMeta(
                        Id: dataSourceId,
                        VectorDimension: 42,
                        VectorsAreSparse: true,
                        PermanentAttributes: new[] {"ShardId2", "ShardId1", "Id"}.ToHashSet(),
                        DataSourceShardingMeta: new DataSourceShardingMeta(
                            ShardersByAttributeKey: new Dictionary<string, IDataSourceAttributeValueSharder>
                            {
                                {"ShardId1", new HashBasedDataSourceAttributeValueSharder(numberOfShards: 7, AttributeValueHasher)},
                                {"ShardId2", new ValueBasedDataSourceAttributeValueSharder(AttributeValueHasher, possibleValues: new HashSet<AttributeValue>())},
                            }),
                        AttributeValueTypes: new Dictionary<string, AttributeValueTypeCode>
                        {
                            {"Id", AttributeValueTypeCode.Int64},
                            {"ShardId1", AttributeValueTypeCode.Guid},
                            {"ShardId2", AttributeValueTypeCode.Bool},
                            {"Payload", AttributeValueTypeCode.String}
                        }),
                    ComparingWithRespectToRuntimeTypes);
        }

        [Test]
        public void ParseDataSourceMeta_NoShards()
        {
            const string dataSourceYaml = @"
vectorDimension: 32
vectorsAreSparse: false
attributeValueTypes:
  Id: Int64
  Payload: DateTime
permanentAttributes: ['Id']
";

            var dataSourceId = new DataSourceId(Name: "Test.Name.Source", Version: "1.0");

            sut.ParseDataSourceMeta((dataSourceId, dataSourceYaml))
                .Should()
                .BeEquivalentTo(
                    new DataSourceMeta(
                        Id: dataSourceId,
                        VectorDimension: 32,
                        VectorsAreSparse: false,
                        PermanentAttributes: new[] {"Id"}.ToHashSet(),
                        DataSourceShardingMeta: new DataSourceShardingMeta(
                            ShardersByAttributeKey: new Dictionary<string, IDataSourceAttributeValueSharder>()),
                        AttributeValueTypes: new Dictionary<string, AttributeValueTypeCode>
                        {
                            {"Id", AttributeValueTypeCode.Int64},
                            {"Payload", AttributeValueTypeCode.DateTime}
                        }),
                    ComparingWithRespectToRuntimeTypes);
        }

        [Test]
        public void ParseIndexMeta()
        {
            const string dataSourceYaml = @"
vectorDimension: 100500
vectorsAreSparse: true
attributeValueTypes:
  Id: Int64
  ShardId: Guid
  SplitId: Bool
  Payload: String
permanentAttributes:
- Id
- ShardId
- SplitId
shardersByAttributeKey:
  ShardId:
    numberOfShards: 3
";
            var indexMetaDto = new IndexMetaYamlParser.IndexMetaDto
            {
                IndexAlgorithm = Algorithms.SparnnIndexCosine,
                IdAttributes = new[] {"Id", "ShardId"},
                SplitAttributes = new[] {"SplitId"},
                ShardsById = new Dictionary<string, IndexMetaYamlParser.IndexShardMetaDto>
                {
                    {
                        "ShardA", new IndexMetaYamlParser.IndexShardMetaDto
                        {
                            ShardsByAttributeKey = new Dictionary<string, IndexMetaYamlParser.IndexAttributeValueShardDto>
                            {
                                {
                                    "ShardId", new IndexMetaYamlParser.IndexAttributeValueShardDto
                                    {
                                        NumberOfShards = 6,
                                        ShardValues = new ushort[] {0, 2, 4}
                                    }
                                }
                            },
                            DataSourceShardsToConsume = new[]
                            {
                                new IndexMetaYamlParser.DataSourceShardSubscriptionDto {ShardingCoordinatesByAttributeKey = new Dictionary<string, ulong?> {{"ShardId", 0}}},
                                new IndexMetaYamlParser.DataSourceShardSubscriptionDto {ShardingCoordinatesByAttributeKey = new Dictionary<string, ulong?> {{"ShardId", 2}}},
                            }
                        }
                    },
                    {
                        "ShardB", new IndexMetaYamlParser.IndexShardMetaDto
                        {
                            ShardsByAttributeKey = new Dictionary<string, IndexMetaYamlParser.IndexAttributeValueShardDto>
                            {
                                {
                                    "ShardId", new IndexMetaYamlParser.IndexAttributeValueShardDto
                                    {
                                        NumberOfShards = 6,
                                        ShardValues = new ushort[] {1, 3, 5}
                                    }
                                }
                            },
                            DataSourceShardsToConsume = new[]
                            {
                                new IndexMetaYamlParser.DataSourceShardSubscriptionDto {ShardingCoordinatesByAttributeKey = new Dictionary<string, ulong?> {{"ShardId", 1}}},
                            }
                        }
                    },
                }
            };

            var indexYaml = yamlSerializer.Serialize(indexMetaDto);
            indexYaml.Should()
                .Be(
                    @"indexAlgorithm: SparnnIndex.Cosine
idAttributes:
- Id
- ShardId
splitAttributes:
- SplitId
shardsById:
  ShardA:
    shardsByAttributeKey:
      ShardId:
        numberOfShards: 6
        shardValues:
        - 0
        - 2
        - 4
    dataSourceShardsToConsume:
    - shardingCoordinatesByAttributeKey:
        ShardId: 0
    - shardingCoordinatesByAttributeKey:
        ShardId: 2
  ShardB:
    shardsByAttributeKey:
      ShardId:
        numberOfShards: 6
        shardValues:
        - 1
        - 3
        - 5
    dataSourceShardsToConsume:
    - shardingCoordinatesByAttributeKey:
        ShardId: 1
");

            var indexId = new IndexId(Name: "Test.Name.Index", Version: "1.0");
            var dataSourceId = new DataSourceId(Name: "Test.Name.Source", Version: "0.1");

            sut.ParseIndexMeta((indexId, indexYaml), (dataSourceId, dataSourceYaml))
                .Should()
                .BeEquivalentTo(
                    new IndexMeta(
                        Id: indexId,
                        DataSourceMeta: new DataSourceMeta(
                            Id: dataSourceId,
                            VectorDimension: 100500,
                            VectorsAreSparse: true,
                            PermanentAttributes: new[] {"ShardId", "SplitId", "Id"}.ToHashSet(),
                            DataSourceShardingMeta: new DataSourceShardingMeta(
                                ShardersByAttributeKey: new Dictionary<string, IDataSourceAttributeValueSharder>
                                {
                                    {"ShardId", new HashBasedDataSourceAttributeValueSharder(numberOfShards: 3, AttributeValueHasher)}
                                }),
                            AttributeValueTypes: new Dictionary<string, AttributeValueTypeCode>
                            {
                                {"Id", AttributeValueTypeCode.Int64},
                                {"ShardId", AttributeValueTypeCode.Guid},
                                {"SplitId", AttributeValueTypeCode.Bool},
                                {"Payload", AttributeValueTypeCode.String}
                            }),
                        IndexAlgorithm: Algorithms.SparnnIndexCosine,
                        IdAttributes: new[] {"Id", "ShardId"}.ToHashSet(),
                        SplitAttributes: new[] {"SplitId"}.ToHashSet(),
                        IndexShardsMap: new IndexShardsMapMeta(
                            ShardsById: new Dictionary<string, IndexShardMeta>
                            {
                                {
                                    "ShardA", new IndexShardMeta(
                                        ShardsByAttributeKey: new Dictionary<string, IIndexAttributeValueShard>
                                        {
                                            {
                                                "ShardId", new IndexAttributeValueShard<ushort>(
                                                    shardingRule: IndexShardingRule.BelongToSet,
                                                    shardValues: new ushort[] {0, 2, 4}.ToHashSet(),
                                                    attributeValueProjector: new ShardAttributeValueProjector(numberOfShards: 6, AttributeValueHasher))
                                            },
                                        },
                                        DataSourceShardsToConsume: new[]
                                        {
                                            new DataSourceShardSubscription(ShardingCoordinatesByAttributeKey: new Dictionary<string, ulong?> {{"ShardId", 0}}),
                                            new DataSourceShardSubscription(ShardingCoordinatesByAttributeKey: new Dictionary<string, ulong?> {{"ShardId", 2}}),
                                        })
                                },
                                {
                                    "ShardB", new IndexShardMeta(
                                        ShardsByAttributeKey: new Dictionary<string, IIndexAttributeValueShard>
                                        {
                                            {
                                                "ShardId", new IndexAttributeValueShard<ushort>(
                                                    shardingRule: IndexShardingRule.BelongToSet,
                                                    shardValues: new ushort[] {1, 3, 5}.ToHashSet(),
                                                    attributeValueProjector: new ShardAttributeValueProjector(numberOfShards: 6, AttributeValueHasher))
                                            },
                                        },
                                        DataSourceShardsToConsume: new[]
                                        {
                                            new DataSourceShardSubscription(ShardingCoordinatesByAttributeKey: new Dictionary<string, ulong?> {{"ShardId", 1}}),
                                        })
                                },
                            })),
                    ComparingWithRespectToRuntimeTypes);
        }

        [Test]
        public void ParseIndexMeta_NoShards()
        {
            const string dataSourceYaml = @"
vectorDimension: 32
vectorsAreSparse: false
attributeValueTypes:
  Id: Int64
permanentAttributes: ['Id']
";

            const string indexYaml = @"
indexAlgorithm: FaissIndex.Flat.L2
idAttributes: ['Id']
splitAttributes: []
shardsById:
  SingleShard: {}
";

            var indexId = new IndexId(Name: "Test.Name.Index", Version: "1.0");
            var dataSourceId = new DataSourceId(Name: "Test.Name.Source", Version: "0.1");

            sut.ParseIndexMeta((indexId, indexYaml), (dataSourceId, dataSourceYaml))
                .Should()
                .BeEquivalentTo(
                    new IndexMeta(
                        Id: indexId,
                        DataSourceMeta: new DataSourceMeta(
                            Id: dataSourceId,
                            VectorDimension: 32,
                            VectorsAreSparse: false,
                            PermanentAttributes: new[] {"Id"}.ToHashSet(),
                            DataSourceShardingMeta: new DataSourceShardingMeta(
                                ShardersByAttributeKey: new Dictionary<string, IDataSourceAttributeValueSharder>()),
                            AttributeValueTypes: new Dictionary<string, AttributeValueTypeCode>
                            {
                                {"Id", AttributeValueTypeCode.Int64}
                            }),
                        IndexAlgorithm: Algorithms.FaissIndexFlatL2,
                        IdAttributes: new[] {"Id"}.ToHashSet(),
                        SplitAttributes: new HashSet<string>(),
                        IndexShardsMap: new IndexShardsMapMeta(
                            ShardsById: new Dictionary<string, IndexShardMeta>
                            {
                                {
                                    "SingleShard", new IndexShardMeta(
                                        ShardsByAttributeKey: new Dictionary<string, IIndexAttributeValueShard>(),
                                        DataSourceShardsToConsume: new[] {new DataSourceShardSubscription(new Dictionary<string, ulong?>())})
                                }
                            })),
                    ComparingWithRespectToRuntimeTypes);
        }

        private static EquivalencyAssertionOptions<T> ComparingWithRespectToRuntimeTypes<T>(EquivalencyAssertionOptions<T> options)
        {
            return options.RespectingRuntimeTypes();
        }
    }
}
