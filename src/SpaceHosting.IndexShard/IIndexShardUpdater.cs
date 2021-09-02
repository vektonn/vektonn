using SpaceHosting.Contracts;
using SpaceHosting.Index;

namespace SpaceHosting.IndexShard
{
    public interface IIndexShardUpdater<TVector>
        where TVector : IVector
    {
        void UpdateIndexShard(DataPointOrTombstone<TVector>[] batch);
    }
}
