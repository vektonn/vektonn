using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Vektonn.SharedImpl.Contracts;
using Vektonn.SharedImpl.Contracts.Sharding;

namespace Vektonn.SharedImpl.Configuration
{
    public class IndexMetaProvider : IIndexMetaProvider, IIndexShardsTopologyProvider
    {
        private readonly IndexMetaYamlParser yamlParser = new(new AttributeValueHasher());
        private readonly ConcurrentDictionary<IndexId, IndexMeta?> indexMetasById = new();
        private readonly ConcurrentDictionary<DataSourceId, DataSourceMeta?> dataSourceMetasById = new();
        private readonly ConcurrentDictionary<IndexId, Dictionary<string, DnsEndPoint>> indexShardEndpointsByIndexId = new();

        private readonly string configBaseDirectory;

        public IndexMetaProvider(string configBaseDirectory)
        {
            this.configBaseDirectory = configBaseDirectory;
        }

        public IndexMeta? TryGetIndexMeta(IndexId indexId)
        {
            return indexMetasById.GetOrAdd(indexId, TryGetIndexMetaImpl);
        }

        public DataSourceMeta? TryGetDataSourceMeta(DataSourceId dataSourceId)
        {
            return dataSourceMetasById.GetOrAdd(dataSourceId, TryGetDataSourceMetaImpl);
        }

        public Dictionary<string, DnsEndPoint> GetEndpointsByShardIdForIndex(IndexId indexId)
        {
            return indexShardEndpointsByIndexId.GetOrAdd(indexId, GetEndpointsByShardIdForIndexImpl);
        }

        private IndexMeta? TryGetIndexMetaImpl(IndexId indexId)
        {
            var indexConfigFileName = FormatIndexConfigFileName(indexId);
            if (!File.Exists(indexConfigFileName))
                return null;

            var indexConfigYaml = ReadConfigYaml(indexConfigFileName);
            var dataSourceId = yamlParser.ParseDataSourceId(indexConfigYaml);
            var dataSourceYaml = ReadConfigYaml(FormatDataSourceConfigFileName(dataSourceId));
            var indexMeta = yamlParser.ParseIndexMeta(
                indexConfig: (indexId, indexConfigYaml),
                dataSourceConfig: (dataSourceId, dataSourceYaml));

            indexMeta.ValidateConsistency();

            return indexMeta;
        }

        private Dictionary<string, DnsEndPoint> GetEndpointsByShardIdForIndexImpl(IndexId indexId)
        {
            var indexMeta = TryGetIndexMeta(indexId);
            if (indexMeta == null)
                throw new InvalidOperationException($"Failed to get indexMeta for indexId: {indexId}");

            var indexShardEndpointsYaml = ReadConfigYaml(Path.Combine(configBaseDirectory, "index-shards-topology.yaml"));
            var indexShardEndpoints = yamlParser.ParseIndexShardEndpoints(indexShardEndpointsYaml);

            if (!indexShardEndpoints.TryGetValue(indexMeta.Id, out var endpointsByShardId))
                throw new InvalidOperationException($"Index shards topology is not defined for indexId: {indexId}");

            foreach (var shardId in indexMeta.IndexShardsMap.ShardsById.Keys)
            {
                if (!endpointsByShardId.ContainsKey(shardId))
                    throw new InvalidOperationException($"Index shard endpoint is not specified for indexId: {indexMeta.Id}, shardId: {shardId}");
            }

            return endpointsByShardId;
        }

        private DataSourceMeta? TryGetDataSourceMetaImpl(DataSourceId dataSourceId)
        {
            var dataSourceConfigFileName = FormatDataSourceConfigFileName(dataSourceId);
            if (!File.Exists(dataSourceConfigFileName))
                return null;

            var dataSourceYaml = ReadConfigYaml(dataSourceConfigFileName);
            var dataSourceMeta = yamlParser.ParseDataSourceMeta(dataSourceConfig: (dataSourceId, dataSourceYaml));

            dataSourceMeta.ValidateConsistency();

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
