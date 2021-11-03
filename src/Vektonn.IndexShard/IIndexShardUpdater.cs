using System.Collections.Generic;
using Vektonn.Index;
using Vektonn.SharedImpl.Contracts;

namespace Vektonn.IndexShard
{
    public interface IIndexShardUpdater<TVector>
        where TVector : IVector
    {
        void UpdateIndexShard(IReadOnlyList<DataPointOrTombstone<TVector>> dataPointOrTombstones);
    }
}
