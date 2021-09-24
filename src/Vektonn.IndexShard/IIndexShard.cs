using System;
using System.Collections.Generic;
using Vektonn.Contracts;
using Vektonn.Index;

namespace Vektonn.IndexShard
{
    internal interface IIndexShard<TVector> : IDisposable
        where TVector : IVector
    {
        long DataPointsCount { get; }

        void UpdateIndex(DataPointOrTombstone<TVector>[] batch);

        IReadOnlyList<SearchResultItem<TVector>> FindNearest(SearchQuery<TVector> query);
    }
}
