using System.Collections.Generic;
using System.Threading;

namespace Vektonn.IndexShard
{
    public interface IDataSourceConsumer
    {
        void SubscribeAndInit(IReadOnlyList<string> sourceTopics, CancellationToken cancellationToken);
    }
}
