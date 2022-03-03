using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vektonn.ApiContracts.Json;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;

namespace Vektonn.ApiClient.HttpClusterClient
{
    internal static class ClusterClientExtensions
    {
        public static Request BuildVektonnRequest(HttpMethod httpMethod, Uri requestUrl)
        {
            return BuildVektonnRequest<object>(httpMethod, requestUrl, requestModel: null);
        }

        public static Request BuildVektonnRequest<TRequest>(HttpMethod httpMethod, Uri requestUrl, TRequest? requestModel)
            where TRequest : class
        {
            Request request;
            if (httpMethod == HttpMethod.Get)
                request = Request.Get(requestUrl);
            else if (httpMethod == HttpMethod.Post)
            {
                request = Request.Post(requestUrl);
                if (requestModel != null)
                {
                    var stringContent = JsonSerializer.Serialize(requestModel, HttpJson.Options);
                    request = request
                        .WithContent(stringContent)
                        .WithContentTypeHeader(MediaTypeNames.Application.Json);
                }
            }
            else
                throw new InvalidOperationException($"Invalid httpMethod: {httpMethod}");

            return request;
        }

        public static async Task GetVoidResponseAsync(
            this IClusterClient clusterClient,
            Request request,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            using var clusterResult = await clusterClient.GetClusterResultAsync(request, timeout, cancellationToken);

            if (clusterResult.Response.IsSuccessful)
                return;

            throw new VektonnClusterClientException(clusterResult);
        }

        public static async Task<TResponse> GetResponseAsync<TResponse>(
            this IClusterClient clusterClient,
            Request request,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            using var clusterResult = await clusterClient.GetClusterResultAsync(request, timeout, cancellationToken);

            if (clusterResult.Response.IsSuccessful)
            {
                var responseString = clusterResult.Response.Content.ToString();
                var responseModel = JsonSerializer.Deserialize<TResponse>(responseString, HttpJson.Options);
                return responseModel ?? throw new InvalidOperationException($"Null response for request: {clusterResult.Request.Url}");
            }

            throw new VektonnClusterClientException(clusterResult);
        }

        private static async Task<ClusterResult> GetClusterResultAsync(
            this IClusterClient clusterClient,
            Request request,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            var clusterResult = await clusterClient.SendAsync(request, parameters: null, timeout, cancellationToken);

            if (clusterResult.Status == ClusterResultStatus.Canceled)
                throw new OperationCanceledException(cancellationToken);

            return clusterResult;
        }
    }
}
