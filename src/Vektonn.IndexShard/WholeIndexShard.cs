using System.Collections.Generic;
using Vektonn.Contracts;
using Vektonn.Index;
using Vostok.Logging.Abstractions;

namespace Vektonn.IndexShard
{
    internal class WholeIndexShard<TVector> : IIndexShard<TVector>
        where TVector : IVector
    {
        private readonly ILog log;
        private readonly AttributesAccessor attributesAccessor;
        private readonly IndexWithLocker<TVector> indexWithLocker;

        public WholeIndexShard(
            ILog log,
            IndexMeta indexMeta,
            IIndexStoreFactory<byte[], byte[]> indexStoreFactory)
        {
            this.log = log;
            attributesAccessor = new AttributesAccessor(indexMeta);
            indexWithLocker = new IndexWithLocker<TVector>(
                indexStoreFactory.Create<TVector>(
                    indexMeta.IndexAlgorithm,
                    indexMeta.VectorDimension,
                    withDataStorage: indexMeta.HasPayload,
                    ByteArrayComparer.Instance));
        }

        public long DataPointsCount => indexWithLocker.DataPointsCount;

        public void UpdateIndex(DataPointOrTombstone<TVector>[] batch)
        {
            var indexDataPointOrTombstones = batch.ToIndexDataPointOrTombstones(attributesAccessor);
            var dataPointsCount = indexWithLocker.UpdateIndex(indexDataPointOrTombstones);
            log.Info($"Added batch to index. Index total count now = {dataPointsCount}");
        }

        public IReadOnlyList<SearchResultItem<TVector>> FindNearest(SearchQuery<TVector> query)
        {
            var indexQueryResults = indexWithLocker.FindNearest(query);
            return indexQueryResults.ToSearchResults(attributesAccessor, splitKeyBytes: null);
        }

        public void Dispose()
        {
            indexWithLocker.Dispose();
        }
    }
}
