using System;
using System.Collections.Concurrent;
using Vektonn.ApiClient.IndexShard;
using Vektonn.SharedImpl.Configuration;
using Vektonn.SharedImpl.Contracts;

namespace Vektonn.ApiService.Services
{
    public class IndexShardApiClientProvider
    {
        private readonly IIndexShardsTopologyProvider indexShardsTopologyProvider;
        private readonly IndexShardApiAbsoluteUriClusterClient indexShardApiAbsoluteUriClusterClient;
        private readonly ConcurrentDictionary<(IndexId IndexId, string ShardId), IndexShardApiClient> indexShardApiClientsByIndexShardId = new();

        public IndexShardApiClientProvider(
            IIndexShardsTopologyProvider indexShardsTopologyProvider,
            IndexShardApiAbsoluteUriClusterClient indexShardApiAbsoluteUriClusterClient)
        {
            this.indexShardsTopologyProvider = indexShardsTopologyProvider;
            this.indexShardApiAbsoluteUriClusterClient = indexShardApiAbsoluteUriClusterClient;
        }

        public IndexShardApiClient GetIndexShardApiClient(IndexId indexId, string shardId)
        {
            return indexShardApiClientsByIndexShardId.GetOrAdd((indexId, shardId), GetIndexShardApiClientImpl);
        }

        private IndexShardApiClient GetIndexShardApiClientImpl((IndexId IndexId, string ShardId) key)
        {
            var endpointsByShardId = indexShardsTopologyProvider.GetEndpointsByShardIdForIndex(key.IndexId);
            var endpoint = endpointsByShardId[key.ShardId];
            var indexShardBaseUri = new Uri($"http://{endpoint.Host}:{endpoint.Port}");

            return new IndexShardApiClient(indexShardApiAbsoluteUriClusterClient, indexShardBaseUri);
        }
    }
}
