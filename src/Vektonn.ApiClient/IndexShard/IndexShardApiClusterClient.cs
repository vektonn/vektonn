using System;
using Vektonn.ApiClient.HttpClusterClient;
using Vostok.Logging.Abstractions;
using Vostok.Tracing.Abstractions;

namespace Vektonn.ApiClient.IndexShard
{
    public class IndexShardApiClusterClient : AbsoluteUriClusterClient
    {
        public IndexShardApiClusterClient(ILog log, ITracer tracer)
            : base(log.ForContext<IndexShardApiClusterClient>(), tracer, defaultRequestTimeout: TimeSpan.FromSeconds(5))
        {
        }
    }
}
