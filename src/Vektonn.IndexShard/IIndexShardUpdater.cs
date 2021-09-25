using System.Collections.Generic;
using Vektonn.Contracts;
using Vektonn.Index;

namespace Vektonn.IndexShard
{
    public interface IIndexShardUpdater<TVector>
        where TVector : IVector
    {
        void UpdateIndexShard(IReadOnlyList<DataPointOrTombstone<TVector>> dataPointOrTombstones);
    }
}
