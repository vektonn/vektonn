using System.Collections.Generic;
using Vektonn.Index;
using Vektonn.SharedImpl.Contracts;
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
                    indexMeta.IndexAlgorithm.Type,
                    indexMeta.VectorDimension,
                    withDataStorage: indexMeta.HasPayload,
                    idComparer: ByteArrayComparer.Instance,
                    indexMeta.IndexAlgorithm.Params));
        }

        public long DataPointsCount => indexWithLocker.DataPointsCount;

        public void UpdateIndex(IReadOnlyList<DataPointOrTombstone<TVector>> dataPointOrTombstones)
        {
            var indexDataPointOrTombstones = dataPointOrTombstones.ToIndexDataPointOrTombstones(attributesAccessor);
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
