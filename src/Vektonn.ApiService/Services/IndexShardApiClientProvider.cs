using System;
using Vektonn.ApiClient.IndexShard;
using Vektonn.SharedImpl.Configuration;
using Vektonn.SharedImpl.Contracts;

namespace Vektonn.ApiService.Services
{
    public class IndexShardApiClientProvider
    {
        private readonly IIndexShardsTopologyProvider indexShardsTopologyProvider;
        private readonly IndexShardApiClusterClient indexShardApiClusterClient;

        public IndexShardApiClientProvider(IIndexShardsTopologyProvider indexShardsTopologyProvider, IndexShardApiClusterClient indexShardApiClusterClient)
        {
            this.indexShardsTopologyProvider = indexShardsTopologyProvider;
            this.indexShardApiClusterClient = indexShardApiClusterClient;
        }

        public IndexShardApiClient GetIndexShardApiClient(IndexId indexId, string shardId)
        {
            var endpointsByShardId = indexShardsTopologyProvider.GetEndpointsByShardIdForIndex(indexId);
            var endpoint = endpointsByShardId[shardId];
            var indexShardBaseUri = new Uri($"http://{endpoint.Host}:{endpoint.Port}");

            return new IndexShardApiClient(indexShardApiClusterClient, indexShardBaseUri);
        }
    }
}
