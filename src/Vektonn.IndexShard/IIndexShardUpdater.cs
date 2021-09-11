using Vektonn.Contracts;
using Vektonn.Index;

namespace Vektonn.IndexShard
{
    public interface IIndexShardUpdater<TVector>
        where TVector : IVector
    {
        void UpdateIndexShard(DataPointOrTombstone<TVector>[] dataPointOrTombstones);
    }
}
