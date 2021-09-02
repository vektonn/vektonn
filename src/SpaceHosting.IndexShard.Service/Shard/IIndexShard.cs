using System;
using System.Collections.Generic;
using SpaceHosting.Index;
using SpaceHosting.IndexShard.Models;

namespace SpaceHosting.IndexShard.Service.Shard
{
    public interface IIndexShard<TVector> : IDisposable
        where TVector : IVector
    {
        void UpdateIndex(DataPointOrTombstone<TVector>[] batch);

        IReadOnlyList<SearchResultItem<TVector>> FindNearest(SearchQuery<TVector> query);
    }
}
