using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Vektonn.SharedImpl.Contracts;
using Vektonn.SharedImpl.Contracts.Sharding;

namespace Vektonn.SharedImpl.Configuration
{
    public class IndexMetaProvider : IIndexMetaProvider
    {
        private readonly IndexMetaYamlParser yamlParser = new(new AttributeValueHasher());
        private readonly ConcurrentDictionary<IndexId, IndexMetaWithShardEndpoints> indexMetasById = new();
        private readonly ConcurrentDictionary<DataSourceId, DataSourceMeta> dataSourceMetasById = new();

        private readonly string configBaseDirectory;

        public IndexMetaProvider(string configBaseDirectory)
        {
            this.configBaseDirectory = configBaseDirectory;
        }

        public IndexMetaWithShardEndpoints? TryGetIndexMeta(IndexId indexId)
        {
            var indexConfigFileName = FormatIndexConfigFileName(indexId);
            if (!File.Exists(indexConfigFileName))
                return null;

            var indexMeta = indexMetasById.GetOrAdd(indexId, _ => GetIndexMeta(indexId, indexConfigFileName));
            return indexMeta;
        }

        public DataSourceMeta? TryGetDataSourceMeta(DataSourceId dataSourceId)
        {
            var dataSourceConfigFileName = FormatDataSourceConfigFileName(dataSourceId);
            if (!File.Exists(dataSourceConfigFileName))
                return null;

            var dataSourceMeta = dataSourceMetasById.GetOrAdd(dataSourceId, _ => GetDataSourceMeta(dataSourceId, dataSourceConfigFileName));
            return dataSourceMeta;
        }

        private IndexMetaWithShardEndpoints GetIndexMeta(IndexId indexId, string indexConfigFileName)
        {
            var indexConfigYaml = ReadConfigYaml(indexConfigFileName);
            var dataSourceId = yamlParser.ParseDataSourceId(indexConfigYaml);
            var dataSourceYaml = ReadConfigYaml(FormatDataSourceConfigFileName(dataSourceId));
            var indexMeta = yamlParser.ParseIndexMeta(
                indexConfig: (indexId, indexConfigYaml),
                dataSourceConfig: (dataSourceId, dataSourceYaml));

            indexMeta.ValidateConsistency();

            var endpointsByShardId = GetEndpointsByShardIdForIndex(indexMeta);
            return new IndexMetaWithShardEndpoints(indexMeta, endpointsByShardId);
        }

        private Dictionary<string, DnsEndPoint> GetEndpointsByShardIdForIndex(IndexMeta indexMeta)
        {
            var indexShardEndpointsYaml = ReadConfigYaml(Path.Combine(configBaseDirectory, "index-shards-topology.yaml"));
            var indexShardEndpoints = yamlParser.ParseIndexShardEndpoints(indexShardEndpointsYaml);

            if (!indexShardEndpoints.TryGetValue(indexMeta.Id, out var endpointsByShardId))
                throw new InvalidOperationException($"Index shards topology is not defined for indexId: {indexMeta.Id}");

            foreach (var shardId in indexMeta.IndexShardsMap.ShardsById.Keys)
            {
                if (!endpointsByShardId.ContainsKey(shardId))
                    throw new InvalidOperationException($"Index shard endpoint is not specified for indexId: {indexMeta.Id}, shardId: {shardId}");
            }

            return endpointsByShardId;
        }

        private DataSourceMeta GetDataSourceMeta(DataSourceId dataSourceId, string dataSourceConfigFileName)
        {
            var dataSourceYaml = ReadConfigYaml(dataSourceConfigFileName);
            var dataSourceMeta = yamlParser.ParseDataSourceMeta(dataSourceConfig: (dataSourceId, dataSourceYaml));
            return dataSourceMeta;
        }

        private static string ReadConfigYaml(string configFileName) =>
            File.ReadAllText(configFileName);

        private string FormatIndexConfigFileName(IndexId indexId) =>
            Path.Combine(configBaseDirectory, "indices", indexId.Name, $"{indexId.Version}.yaml");

        private string FormatDataSourceConfigFileName(DataSourceId dataSourceId) =>
            Path.Combine(configBaseDirectory, "data-sources", dataSourceId.Name, $"{dataSourceId.Version}.yaml");
    }
}
