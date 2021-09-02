using System.Collections.Generic;
using SpaceHosting.Index;
using SpaceHosting.IndexShard.Models;
using Vostok.Logging.Abstractions;

namespace SpaceHosting.IndexShard.Service.Shard
{
    public class IndexShard<TVector> : IIndexShard<TVector>
        where TVector : IVector
    {
        private readonly ILog log;
        private readonly AttributesAccessor attributesAccessor;
        private readonly IndexWithLocker<TVector> indexWithLocker;

        public IndexShard(
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
