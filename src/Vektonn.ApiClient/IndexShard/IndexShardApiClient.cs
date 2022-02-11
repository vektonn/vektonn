using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Vektonn.ApiContracts;
using Vostok.Clusterclient.Core.Model;

namespace Vektonn.ApiClient.IndexShard
{
    public class IndexShardApiClient
    {
        private readonly Uri indexShardBaseUri;
        private readonly IndexShardApiClusterClient clusterClient;

        public IndexShardApiClient(IndexShardApiClusterClient clusterClient, Uri indexShardBaseUri)
        {
            this.clusterClient = clusterClient;
            this.indexShardBaseUri = indexShardBaseUri;
        }

        public async Task<IndexInfoDto> GetIndexInfoAsync(
            CancellationToken cancellationToken = default,
            TimeSpan? timeout = null)
        {
            var requestUrl = new Uri(indexShardBaseUri, "api/v1/info");
            var request = clusterClient.BuildRequest(HttpMethod.Get, requestUrl);

            return await clusterClient.GetResponseAsync<IndexInfoDto>(request, cancellationToken, timeout);
        }

        public async Task<SearchResultDto> ProbeAsync(
            int? k = null,
            CancellationToken cancellationToken = default,
            TimeSpan? timeout = null)
        {
            var requestUrl = new Uri(indexShardBaseUri, "api/v1/probe");
            var request = clusterClient.BuildRequest(HttpMethod.Get, requestUrl);
            if (k != null)
                request = request.WithAdditionalQueryParameter(nameof(k), k.Value);

            return await clusterClient.GetResponseAsync<SearchResultDto>(request, cancellationToken, timeout);
        }

        public async Task<SearchResultDto[]> SearchAsync(
            SearchQueryDto searchQuery,
            CancellationToken cancellationToken = default,
            TimeSpan? timeout = null)
        {
            var requestUrl = new Uri(indexShardBaseUri, "api/v1/search");
            var request = clusterClient.BuildRequest(HttpMethod.Post, requestUrl, searchQuery);

            return await clusterClient.GetResponseAsync<SearchResultDto[]>(request, cancellationToken, timeout);
        }
    }
}
