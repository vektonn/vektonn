using System;
using System.Collections.Generic;
using System.Net;
using Vektonn.ApiClient.IndexShard;

namespace Vektonn.ApiService.Services
{
    public class IndexShardApiClientProvider
    {
        private readonly IndexShardApiClusterClient indexShardApiClusterClient;

        public IndexShardApiClientProvider(IndexShardApiClusterClient indexShardApiClusterClient)
        {
            this.indexShardApiClusterClient = indexShardApiClusterClient;
        }

        public IndexShardApiClient GetIndexShardApiClient(Dictionary<string, DnsEndPoint> endpointsByShardId, string shardId)
        {
            var endpoint = endpointsByShardId[shardId];
            var indexShardBaseUri = new Uri($"http://{endpoint.Host}:{endpoint.Port}");

            return new IndexShardApiClient(indexShardApiClusterClient, indexShardBaseUri);
        }
    }
}
