using System;
using System.Threading;
using System.Threading.Tasks;
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
    public abstract class AbsoluteUriClusterClient : IClusterClient
    {
        private readonly IClusterClient clusterClient;

        protected AbsoluteUriClusterClient(ILog log, ITracer tracer, TimeSpan? defaultRequestTimeout = null)
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

                    if (defaultRequestTimeout != null)
                        configuration.DefaultTimeout = defaultRequestTimeout.Value;

                    configuration.DefaultConnectionTimeout = TimeSpan.FromSeconds(1);

                    configuration.ClusterProvider = new AdHocClusterProvider(() => null);
                    configuration.DefaultRequestStrategy = Strategy.SingleReplica;

                    configuration.SetupDistributedTracing(tracer);
                });
        }

        public Task<ClusterResult> SendAsync(
            Request request,
            RequestParameters? parameters = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            return clusterClient.SendAsync(request, parameters, timeout, cancellationToken);
        }
    }
}
