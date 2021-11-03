using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vektonn.ApiContracts.Json;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Tracing;
using Vostok.Clusterclient.Transport;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Tracing;
using Vostok.Tracing.Abstractions;

namespace Vektonn.ApiClient.HttpClusterClient
{
    public abstract class AbsoluteUriClusterClient
    {
        private readonly IClusterClient clusterClient;

        protected AbsoluteUriClusterClient(ILog log, ITracer tracer, TimeSpan defaultRequestTimeout)
        {
            clusterClient = new ClusterClient(
                log.WithTracingProperties(tracer),
                configuration =>
                {
                    configuration.SetupUniversalTransport(
                        new UniversalTransportSettings
                        {
                            TcpKeepAliveEnabled = true
                        });
                    configuration.DefaultTimeout = defaultRequestTimeout;
                    configuration.DefaultConnectionTimeout = TimeSpan.FromSeconds(1);

                    configuration.ClusterProvider = new AdHocClusterProvider(() => null);
                    configuration.DefaultRequestStrategy = Strategy.SingleReplica;

                    configuration.SetupDistributedTracing(tracer);
                });
        }

        public Request BuildRequest(HttpMethod httpMethod, Uri requestUrl)
        {
            return BuildRequest<object>(httpMethod, requestUrl, requestModel: null);
        }

        public Request BuildRequest<TRequest>(HttpMethod httpMethod, Uri requestUrl, TRequest? requestModel)
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

        public async Task GetVoidResponseAsync(Request request, CancellationToken cancellationToken = default, TimeSpan? timeout = null)
        {
            using var clusterResult = await GetClusterResultAsync(request, cancellationToken, timeout);

            if (clusterResult.Response.IsSuccessful)
                return;

            throw new VektonnClusterClientException(clusterResult);
        }

        public async Task<TResponse> GetResponseAsync<TResponse>(Request request, CancellationToken cancellationToken = default, TimeSpan? timeout = null)
        {
            using var clusterResult = await GetClusterResultAsync(request, cancellationToken, timeout);

            if (clusterResult.Response.IsSuccessful)
            {
                var responseString = clusterResult.Response.Content.ToString();
                var responseModel = JsonSerializer.Deserialize<TResponse>(responseString, HttpJson.Options);
                return responseModel ?? throw new InvalidOperationException($"Null response for request: {clusterResult.Request.Url}");
            }

            throw new VektonnClusterClientException(clusterResult);
        }

        public async Task<ClusterResult> GetClusterResultAsync(Request request, CancellationToken cancellationToken = default, TimeSpan? timeout = null)
        {
            if (!request.Url.IsAbsoluteUri)
                throw new InvalidOperationException($"request.Url does not represent an absolute Uri: {request.Url}");

            var clusterResult = await clusterClient.SendAsync(request, cancellationToken: cancellationToken, timeout: timeout);
            if (clusterResult.Status == ClusterResultStatus.Canceled)
                throw new OperationCanceledException(cancellationToken);

            return clusterResult;
        }
    }
}
