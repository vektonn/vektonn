using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Vektonn.SharedImpl.Contracts;
using Vektonn.SharedImpl.Contracts.Sharding;
using Vektonn.SharedImpl.Contracts.Sharding.DataSource;
using Vektonn.SharedImpl.Contracts.Sharding.Index;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Vektonn.SharedImpl.Configuration
{
    [SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
    public class IndexMetaYamlParser
    {
        private readonly IAttributeValueHasher attributeValueHasher;
        private readonly IDeserializer yamlDeserializer;

        public IndexMetaYamlParser(IAttributeValueHasher attributeValueHasher)
        {
            this.attributeValueHasher = attributeValueHasher;

            yamlDeserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
        }

        public DataSourceId ParseDataSourceId(string yaml)
        {
            var dto = yamlDeserializer.Deserialize<DataSourceReferenceDto>(yaml);
            return new DataSourceId(dto.DataSourceId.Name, dto.DataSourceId.Version);
        }

        [SuppressMessage("ReSharper", "ConstantNullCoalescingCondition")]
        public IndexMeta ParseIndexMeta(
            (IndexId Id, string Yaml) indexConfig,
            (DataSourceId Id, string Yaml) dataSourceConfig)
        {
            var dto = yamlDeserializer.Deserialize<IndexMetaDto>(indexConfig.Yaml);

            var dataSourceMeta = ParseDataSourceMeta(dataSourceConfig);

            return new IndexMeta(
                Id: indexConfig.Id,
                DataSourceMeta: dataSourceMeta,
                IndexAlgorithm: dto.IndexAlgorithm,
                IdAttributes: dto.IdAttributes.ToHashSet(),
                SplitAttributes: dto.SplitAttributes.ToHashSet(),
                IndexShardsMap: new IndexShardsMapMeta(
                    ShardsById: dto.ShardsById.ToDictionary(
                        t => t.Key,
                        t =>
                        {
                            var indexShardMetaDto = t.Value ?? new IndexShardMetaDto();
                            return new IndexShardMeta(
                                ShardsByAttributeKey: indexShardMetaDto.ShardsByAttributeKey.ToDictionary(
                                    u => u.Key,
                                    u =>
                                    {
                                        var indexAttributeValueShardDto = u.Value ?? new IndexAttributeValueShardDto();
                                        return (IIndexAttributeValueShard)new IndexAttributeValueShard<ushort>(
                                            shardingRule: IndexShardingRule.BelongToSet,
                                            shardValues: indexAttributeValueShardDto.ShardValues.ToHashSet(),
                                            attributeValueProjector: new ShardAttributeValueProjector(
                                                numberOfShards: indexAttributeValueShardDto.NumberOfShards,
                                                attributeValueHasher));
                                    }),
                                DataSourceShardsToConsume: indexShardMetaDto.DataSourceShardsToConsume
                                    .Select(x => new DataSourceShardSubscription(x.ShardingCoordinatesByAttributeKey))
                                    .ToArray());
                        })));
        }

        [SuppressMessage("ReSharper", "ConstantConditionalAccessQualifier")]
        public DataSourceMeta ParseDataSourceMeta((DataSourceId Id, string Yaml) dataSourceConfig)
        {
            var dto = yamlDeserializer.Deserialize<DataSourceMetaDto>(dataSourceConfig.Yaml);

            return new DataSourceMeta(
                Id: dataSourceConfig.Id,
                VectorDimension: dto.VectorDimension,
                VectorsAreSparse: dto.VectorsAreSparse,
                PermanentAttributes: dto.PermanentAttributes.ToHashSet(),
                DataSourceShardingMeta: new DataSourceShardingMeta(
                    ShardersByAttributeKey: dto.ShardersByAttributeKey.ToDictionary(
                        t => t.Key,
                        t => t.Value?.NumberOfShards != null
                            ? new HashBasedDataSourceAttributeValueSharder(
                                numberOfShards: t.Value.NumberOfShards.Value,
                                attributeValueHasher)
                            : (IDataSourceAttributeValueSharder)new ValueBasedDataSourceAttributeValueSharder(
                                attributeValueHasher,
                                possibleValues: new HashSet<AttributeValue>()))),
                AttributeValueTypes: dto.AttributeValueTypes);
        }

        public class DataSourceReferenceDto
        {
            public IdDto DataSourceId { get; set; } = new IdDto();
        }

        public class IdDto
        {
            public string Name { get; set; } = string.Empty;
            public string Version { get; set; } = string.Empty;
        }

        public class IndexMetaDto
        {
            public string IndexAlgorithm { get; set; } = string.Empty;
            public string[] IdAttributes { get; set; } = Array.Empty<string>();
            public string[] SplitAttributes { get; set; } = Array.Empty<string>();
            public Dictionary<string, IndexShardMetaDto> ShardsById { get; set; } = new Dictionary<string, IndexShardMetaDto>();
        }

        public class IndexShardMetaDto
        {
            public Dictionary<string, IndexAttributeValueShardDto> ShardsByAttributeKey { get; set; } = new Dictionary<string, IndexAttributeValueShardDto>();
            public DataSourceShardSubscriptionDto[] DataSourceShardsToConsume { get; set; } = {new DataSourceShardSubscriptionDto()};
        }

        // todo (andrew, 29.10.2021): support value-based sharding scheme
        public class IndexAttributeValueShardDto
        {
            public ushort NumberOfShards { get; set; }
            public ushort[] ShardValues { get; set; } = Array.Empty<ushort>();
        }

        public class DataSourceShardSubscriptionDto
        {
            public Dictionary<string, ulong?> ShardingCoordinatesByAttributeKey { get; set; } = new Dictionary<string, ulong?>();
        }

        public class DataSourceMetaDto
        {
            public int VectorDimension { get; set; }
            public bool VectorsAreSparse { get; set; }
            public Dictionary<string, AttributeValueTypeCode> AttributeValueTypes { get; set; } = new Dictionary<string, AttributeValueTypeCode>();
            public string[] PermanentAttributes { get; set; } = Array.Empty<string>();
            public Dictionary<string, DataSourceAttributeValueSharderDto> ShardersByAttributeKey { get; set; } = new Dictionary<string, DataSourceAttributeValueSharderDto>();
        }

        public class DataSourceAttributeValueSharderDto
        {
            public ushort? NumberOfShards { get; set; }
        }
    }
}
