using System;
using SpaceHosting.Contracts;
using SpaceHosting.Contracts.ApiModels;
using SpaceHosting.Index;
using SpaceHosting.IndexShard;

namespace SpaceHosting.Service.IndexShard
{
    public class IndexShardAccessor<TVector> : IDisposable, IIndexShardAccessor
        where TVector : IVector
    {
        private readonly IndexShardHolder<TVector> indexShardHolder;

        public IndexShardAccessor(IndexShardHolder<TVector> indexShardHolder, TVector zeroVector)
        {
            this.indexShardHolder = indexShardHolder;
            ZeroVector = zeroVector.ToVectorDto();
        }

        public IndexMeta IndexMeta => indexShardHolder.IndexMeta;

        public ISearchQueryExecutor SearchQueryExecutor => indexShardHolder;

        public long DataPointsCount => indexShardHolder.DataPointsCount;

        public VectorDto ZeroVector { get; }

        public void Dispose()
        {
            indexShardHolder.Dispose();
        }
    }
}
