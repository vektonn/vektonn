using Vektonn.ApiClient.HttpClusterClient;
using Vostok.Logging.Abstractions;
using Vostok.Tracing.Abstractions;

namespace Vektonn.ApiClient.IndexShard
{
    public class IndexShardApiAbsoluteUriClusterClient : AbsoluteUriClusterClient
    {
        public IndexShardApiAbsoluteUriClusterClient(ILog log, ITracer tracer)
            : base(log.ForContext<IndexShardApiAbsoluteUriClusterClient>(), tracer)
        {
        }
    }
}
