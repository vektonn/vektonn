using System;
using System.Collections.Generic;
using Vektonn.Index;
using Vektonn.SharedImpl.Contracts;

namespace Vektonn.IndexShard
{
    internal interface IIndexShard<TVector> : IDisposable
        where TVector : IVector
    {
        long DataPointsCount { get; }

        void UpdateIndex(IReadOnlyList<DataPointOrTombstone<TVector>> dataPointOrTombstones);

        IReadOnlyList<SearchResultItem<TVector>> FindNearest(SearchQuery<TVector> query);
    }
}
