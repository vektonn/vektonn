using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Vektonn.ApiContracts;
using Vostok.Logging.Abstractions;
using Vostok.Tracing.Abstractions;

namespace Vektonn.ApiClient
{
    public class VektonnApiClient
    {
        private readonly Uri baseUri;
        private readonly VektonnApiClusterClient clusterClient;

        public VektonnApiClient(Uri baseUri, ILog log, TimeSpan? defaultRequestTimeout = null)
            : this(
                baseUri,
                new VektonnApiClusterClient(
                    log,
                    new DevNullTracer(),
                    defaultRequestTimeout ?? TimeSpan.FromSeconds(10)))
        {
        }

        public VektonnApiClient(Uri baseUri, VektonnApiClusterClient clusterClient)
        {
            this.baseUri = baseUri;
            this.clusterClient = clusterClient;
        }

        public async Task<SearchResultDto[]> SearchAsync(
            string indexName,
            string indexVersion,
            SearchQueryDto searchQuery,
            CancellationToken cancellationToken = default,
            TimeSpan? timeout = null)
        {
            var requestUrl = new Uri(baseUri, $"api/v1/search/{indexName}/{indexVersion}");
            var request = clusterClient.BuildRequest(HttpMethod.Post, requestUrl, searchQuery);

            return await clusterClient.GetResponseAsync<SearchResultDto[]>(request, cancellationToken, timeout);
        }

        public async Task UploadAsync(
            string dataSourceName,
            string dataSourceVersion,
            InputDataPointDto[] uploadQuery,
            CancellationToken cancellationToken = default,
            TimeSpan? timeout = null)
        {
            var requestUrl = new Uri(baseUri, $"api/v1/upload/{dataSourceName}/{dataSourceVersion}");
            var request = clusterClient.BuildRequest(HttpMethod.Post, requestUrl, uploadQuery);

            await clusterClient.GetVoidResponseAsync(request, cancellationToken, timeout);
        }
    }
}
