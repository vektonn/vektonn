using SpaceHosting.Index;
using SpaceHosting.IndexShard.Models;

namespace SpaceHosting.IndexShard.Service
{
    public interface IIndexShardUpdater<TVector>
        where TVector : IVector
    {
        void UpdateIndexShard(DataPointOrTombstone<TVector>[] batch);
    }
}
