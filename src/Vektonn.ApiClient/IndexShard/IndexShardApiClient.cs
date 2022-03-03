using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Vektonn.ApiClient.HttpClusterClient;
using Vektonn.ApiContracts;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;

namespace Vektonn.ApiClient.IndexShard
{
    public class IndexShardApiClient
    {
        private readonly IClusterClient clusterClient;
        private readonly IRequestUrlBuilder requestUrlBuilder;

        public IndexShardApiClient(IClusterClient clusterClient)
            : this(clusterClient, new RelativeUriRequestUrlBuilder())
        {
        }

        public IndexShardApiClient(IndexShardApiAbsoluteUriClusterClient absoluteUriClusterClient, Uri indexShardBaseUri)
            : this(absoluteUriClusterClient, new AbsoluteUriRequestUrlBuilder(indexShardBaseUri))
        {
        }

        private IndexShardApiClient(IClusterClient clusterClient, IRequestUrlBuilder requestUrlBuilder)
        {
            this.clusterClient = clusterClient;
            this.requestUrlBuilder = requestUrlBuilder;
        }

        public async Task<IndexInfoDto> GetIndexInfoAsync(
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            var requestUrl = requestUrlBuilder.BuildRequestUrl(path: "api/v1/info");
            var request = ClusterClientExtensions.BuildVektonnRequest(HttpMethod.Get, requestUrl);

            return await clusterClient.GetResponseAsync<IndexInfoDto>(request, timeout, cancellationToken);
        }

        public async Task<SearchResultDto> ProbeAsync(
            int? k = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            var requestUrl = requestUrlBuilder.BuildRequestUrl(path: "api/v1/probe");
            var request = ClusterClientExtensions.BuildVektonnRequest(HttpMethod.Get, requestUrl);
            if (k != null)
                request = request.WithAdditionalQueryParameter(nameof(k), k.Value);

            return await clusterClient.GetResponseAsync<SearchResultDto>(request, timeout, cancellationToken);
        }

        public async Task<SearchResultDto[]> SearchAsync(
            SearchQueryDto searchQuery,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            var requestUrl = requestUrlBuilder.BuildRequestUrl(path: "api/v1/search");
            var request = ClusterClientExtensions.BuildVektonnRequest(HttpMethod.Post, requestUrl, searchQuery);

            return await clusterClient.GetResponseAsync<SearchResultDto[]>(request, timeout, cancellationToken);
        }
    }
}
