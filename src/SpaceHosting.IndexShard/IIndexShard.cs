using System;
using System.Collections.Generic;
using SpaceHosting.Contracts;
using SpaceHosting.Index;

namespace SpaceHosting.IndexShard
{
    internal interface IIndexShard<TVector> : IDisposable
        where TVector : IVector
    {
        long DataPointsCount { get; }

        void UpdateIndex(DataPointOrTombstone<TVector>[] batch);

        IReadOnlyList<SearchResultItem<TVector>> FindNearest(SearchQuery<TVector> query);
    }
}
