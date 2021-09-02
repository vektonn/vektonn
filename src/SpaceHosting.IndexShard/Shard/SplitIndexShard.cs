using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SpaceHosting.Contracts;
using SpaceHosting.Contracts.ResultsMerging;
using SpaceHosting.Index;
using Vostok.Logging.Abstractions;

namespace SpaceHosting.IndexShard.Shard
{
    public class SplitIndexShard<TVector> : IIndexShard<TVector>
        where TVector : IVector
    {
        private readonly ConcurrentDictionary<byte[], IndexWithLocker<TVector>> indexesBySplitKey = new(ByteArrayComparer.Instance);
        private readonly ByPropertyComparer<FoundDataPoint<TVector>, double> foundDataPointsComparer = new(x => x.Distance);

        private readonly ILog log;
        private readonly IndexMeta indexMeta;
        private readonly IIndexStoreFactory<byte[], byte[]> indexStoreFactory;
        private readonly AttributesAccessor attributesAccessor;
        private readonly bool allSplitAttributesAreInIdAttributes;

        private int processedDataPointsTotalCount;

        public SplitIndexShard(
            ILog log,
            IndexMeta indexMeta,
            IIndexStoreFactory<byte[], byte[]> indexStoreFactory)
        {
            this.log = log;
            this.indexMeta = indexMeta;
            this.indexStoreFactory = indexStoreFactory;

            attributesAccessor = new AttributesAccessor(indexMeta);
            allSplitAttributesAreInIdAttributes = !indexMeta.SplitAttributes.Except(indexMeta.IdAttributes).Any();
        }

        public void UpdateIndex(DataPointOrTombstone<TVector>[] batch)
        {
            if (!batch.Any())
                return;

            if (allSplitAttributesAreInIdAttributes || batch.All(x => x.Tombstone == null))
                UpdateSplitBySplit(batch);
            else
                UpdateForInefficientIndexSchema(batch);

            processedDataPointsTotalCount += batch.Length;
            log.Info(
                "Added batch to index: " +
                $"processedDataPoints = {batch.Length}, " +
                $"indexesCount = {indexesBySplitKey.Count}, " +
                $"indexPointsTotalCount = {indexesBySplitKey.Values.Sum(x => x.DataPointsCount)}, " +
                $"processedDataPointsTotalCount = {processedDataPointsTotalCount}");
        }

        public IReadOnlyList<SearchResultItem<TVector>> FindNearest(SearchQuery<TVector> query)
        {
            var partialSplitKey = attributesAccessor.GetPartialSplitKey(query.SplitFilter);

            if (partialSplitKey.Any(x => x == null))
                return FindNearestInAllSplits(query, partialSplitKey);

            var splitKeyBytes = AttributeValueSerializer.Serialize(partialSplitKey!);
            if (indexesBySplitKey.TryGetValue(splitKeyBytes, out var indexWithLocker))
                return FindNearest(query, splitKeyBytes, indexWithLocker);

            return EmptyResponse(query);
        }

        public void Dispose()
        {
            foreach (var indexWithLocker in indexesBySplitKey.Values)
                indexWithLocker.Dispose();
        }

        private void UpdateForInefficientIndexSchema(DataPointOrTombstone<TVector>[] batch)
        {
            var homogeneousBatch = new List<DataPointOrTombstone<TVector>>();
            for (var i = 0; i < batch.Length; i++)
            {
                homogeneousBatch.Add(batch[i]);

                if (i + 1 == batch.Length ||
                    batch[i].Tombstone == null && batch[i + 1].Tombstone != null ||
                    batch[i].Tombstone != null && batch[i + 1].Tombstone == null)
                {
                    UpdateWithHomogeneousBatch(homogeneousBatch);
                    homogeneousBatch.Clear();
                }
            }
        }

        private void UpdateWithHomogeneousBatch(List<DataPointOrTombstone<TVector>> homogeneousBatch)
        {
            if (homogeneousBatch.Last().Tombstone == null)
                UpdateSplitBySplit(homogeneousBatch.ToArray());
            else
            {
                log.Warn($"Scanning all splits to process {homogeneousBatch.Count} tombstones for index with non-optimal schema");

                foreach (var (splitKey, indexWithLocker) in indexesBySplitKey)
                    UpdateIndexForSplitKey(splitKey, indexWithLocker, homogeneousBatch);
            }
        }

        private void UpdateSplitBySplit(DataPointOrTombstone<TVector>[] batch)
        {
            foreach (var dataPointsBySplitKey in batch.GroupBy(GetSplitKey, ByteArrayComparer.Instance))
            {
                var splitKey = dataPointsBySplitKey.Key;
                indexesBySplitKey.TryGetValue(splitKey, out var indexWithLocker);

                if (indexWithLocker == null)
                {
                    if (dataPointsBySplitKey.All(x => x.Tombstone != null))
                    {
                        log.Info("Skip index creation, because all data points are deleted");
                        continue;
                    }

                    indexWithLocker = new IndexWithLocker<TVector>(
                        indexStoreFactory.Create<TVector>(
                            indexMeta.IndexAlgorithm,
                            indexMeta.VectorDimension,
                            withDataStorage: indexMeta.HasPayload,
                            ByteArrayComparer.Instance));

                    indexesBySplitKey.TryAdd(splitKey, indexWithLocker);
                }

                UpdateIndexForSplitKey(splitKey, indexWithLocker, dataPointsBySplitKey);
            }
        }

        private void UpdateIndexForSplitKey(
            byte[] splitKey,
            IndexWithLocker<TVector> indexWithLocker,
            IEnumerable<DataPointOrTombstone<TVector>> dataPointOrTombstones)
        {
            var indexDataPointOrTombstones = dataPointOrTombstones.ToIndexDataPointOrTombstones(attributesAccessor);

            var dataPointsCount = indexWithLocker.UpdateIndex(indexDataPointOrTombstones);

            if (dataPointsCount == 0)
            {
                log.Info("Remove index because all data points are deleted");
                if (indexesBySplitKey.TryRemove(splitKey, out var indexToDispose))
                    indexToDispose.Dispose();
            }
        }

        private byte[] GetSplitKey(DataPointOrTombstone<TVector> dataPointOrTombstone)
        {
            var splitKey = attributesAccessor.GetSplitKey(dataPointOrTombstone.GetAttributes());
            return AttributeValueSerializer.Serialize(splitKey);
        }

        private IReadOnlyList<SearchResultItem<TVector>> FindNearestInAllSplits(SearchQuery<TVector> query, AttributeValue?[] partialSplitKey)
        {
            log.Warn($"Scanning all splits for non-optimal search query: {query}");

            var indexQueryResultsPerIndex = indexesBySplitKey
                .Where(t => SplitKeyMatches(splitKeyBytes: t.Key, partialSplitKey))
                .Select(t => FindNearest(query, splitKeyBytes: t.Key, indexWithLocker: t.Value))
                .ToArray();

            return indexQueryResultsPerIndex.Any()
                ? MergeSearchResults(query, indexQueryResultsPerIndex)
                : EmptyResponse(query);
        }

        private static SearchResultItem<TVector>[] EmptyResponse(SearchQuery<TVector> query)
        {
            return query
                .QueryVectors
                .Select(queryVector => new SearchResultItem<TVector>(queryVector, Array.Empty<FoundDataPoint<TVector>>()))
                .ToArray();
        }

        private static bool SplitKeyMatches(byte[] splitKeyBytes, AttributeValue?[] partialSplitKey)
        {
            if (partialSplitKey.All(x => x == null))
                return true;

            var splitKey = AttributeValueSerializer.Deserialize(splitKeyBytes);

            for (var i = 0; i < splitKey.Length; i++)
            {
                if (partialSplitKey[i] == null)
                    continue;

                if (partialSplitKey[i] != splitKey[i])
                    return false;
            }

            return true;
        }

        private IReadOnlyList<SearchResultItem<TVector>> FindNearest(SearchQuery<TVector> query, byte[] splitKeyBytes, IndexWithLocker<TVector> indexWithLocker)
        {
            var indexQueryResults = indexWithLocker.FindNearest(query);
            return indexQueryResults.ToSearchResults(attributesAccessor, splitKeyBytes);
        }

        private IReadOnlyList<SearchResultItem<TVector>> MergeSearchResults(SearchQuery<TVector> query, IReadOnlyList<SearchResultItem<TVector>>[] searchResults)
        {
            var foundDataPoints = searchResults.Select(r => r.Select(x => x.NearestDataPoints).ToArray());
            var mergeSortDirection = AlgorithmTraits.GetMergeSortDirection(indexMeta.IndexAlgorithm);
            var mergedDataPoints = FoundDataPointsMerger.Merge(foundDataPoints, query.K, mergeSortDirection, foundDataPointsComparer);
            return query.QueryVectors.Zip(
                    mergedDataPoints,
                    (queryVector, nearestDataPoints) => new SearchResultItem<TVector>(queryVector, nearestDataPoints))
                .ToArray();
        }
    }
}
