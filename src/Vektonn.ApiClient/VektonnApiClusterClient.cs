using System;
using Vektonn.ApiClient.HttpClusterClient;
using Vostok.Logging.Abstractions;
using Vostok.Tracing.Abstractions;

namespace Vektonn.ApiClient
{
    public class VektonnApiClusterClient : AbsoluteUriClusterClient
    {
        public VektonnApiClusterClient(ILog log, ITracer tracer, TimeSpan defaultRequestTimeout)
            : base(log.ForContext<VektonnApiClusterClient>(), tracer, defaultRequestTimeout)
        {
        }
    }
}
