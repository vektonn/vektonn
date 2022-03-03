using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Vektonn.ApiClient.HttpClusterClient;
using Vektonn.ApiContracts;
using Vostok.Clusterclient.Core;
using Vostok.Logging.Abstractions;
using Vostok.Tracing.Abstractions;

namespace Vektonn.ApiClient
{
    public class VektonnApiClient
    {
        private readonly IClusterClient clusterClient;
        private readonly IRequestUrlBuilder requestUrlBuilder;

        public VektonnApiClient(IClusterClient clusterClient)
            : this(clusterClient, new RelativeUriRequestUrlBuilder())
        {
        }

        public VektonnApiClient(Uri baseUri, ILog log, TimeSpan? defaultRequestTimeout = null)
            : this(
                new VektonnApiClusterClient(
                    log,
                    new DevNullTracer(),
                    defaultRequestTimeout ?? TimeSpan.FromSeconds(10)),
                new AbsoluteUriRequestUrlBuilder(baseUri))
        {
        }

        private VektonnApiClient(IClusterClient clusterClient, IRequestUrlBuilder requestUrlBuilder)
        {
            this.clusterClient = clusterClient;
            this.requestUrlBuilder = requestUrlBuilder;
        }

        public async Task<SearchResultDto[]> SearchAsync(
            string indexName,
            string indexVersion,
            SearchQueryDto searchQuery,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            var requestUrl = requestUrlBuilder.BuildRequestUrl(path: $"api/v1/search/{indexName}/{indexVersion}");
            var request = ClusterClientExtensions.BuildVektonnRequest(HttpMethod.Post, requestUrl, searchQuery);

            return await clusterClient.GetResponseAsync<SearchResultDto[]>(request, timeout, cancellationToken);
        }

        public async Task UploadAsync(
            string dataSourceName,
            string dataSourceVersion,
            InputDataPointDto[] uploadQuery,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            var requestUrl = requestUrlBuilder.BuildRequestUrl(path: $"api/v1/upload/{dataSourceName}/{dataSourceVersion}");
            var request = ClusterClientExtensions.BuildVektonnRequest(HttpMethod.Post, requestUrl, uploadQuery);

            await clusterClient.GetVoidResponseAsync(request, timeout, cancellationToken);
        }
    }
}
