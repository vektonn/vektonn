using System;
using System.Collections.Generic;
using SpaceHosting.Contracts;
using SpaceHosting.Index;

namespace SpaceHosting.IndexShard.Shard
{
    public interface IIndexShard<TVector> : IDisposable
        where TVector : IVector
    {
        void UpdateIndex(DataPointOrTombstone<TVector>[] batch);

        IReadOnlyList<SearchResultItem<TVector>> FindNearest(SearchQuery<TVector> query);
    }
}
