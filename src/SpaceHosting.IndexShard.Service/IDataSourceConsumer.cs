using System.Collections.Generic;
using System.Threading;

namespace SpaceHosting.IndexShard.Service
{
    public interface IDataSourceConsumer
    {
        void SubscribeAndInit(IReadOnlyList<string> sourceTopics, CancellationToken cancellationToken);
    }
}
