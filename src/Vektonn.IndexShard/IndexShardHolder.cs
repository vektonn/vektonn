using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentValidation.Results;
using Vektonn.ApiContracts;
using Vektonn.Index;
using Vektonn.SharedImpl.ApiContracts;
using Vektonn.SharedImpl.ApiContracts.Validation;
using Vektonn.SharedImpl.Contracts;
using Vostok.Logging.Abstractions;

namespace Vektonn.IndexShard
{
    public class IndexShardHolder<TVector> : IDisposable, IIndexShardUpdater<TVector>, ISearchQueryExecutor
        where TVector : IVector
    {
        private readonly ILog log;
        private readonly SearchQueryValidator searchQueryValidator;
        private readonly IIndexShard<TVector> indexShard;
        private bool isDisposed;

        public IndexShardHolder(ILog log, IndexMeta indexMeta)
        {
            this.log = log.ForContext("VektonnIndexShard");

            IndexMeta = indexMeta;

            searchQueryValidator = new SearchQueryValidator(indexMeta);

            var indexStoreFactory = new IndexStoreFactory<byte[], byte[]>(this.log);

            indexShard = indexMeta.HasSplits
                ? new SplitIndexShard<TVector>(this.log, indexMeta, indexStoreFactory)
                : new WholeIndexShard<TVector>(this.log, indexMeta, indexStoreFactory);
        }

        public IndexMeta IndexMeta { get; }

        public long DataPointsCount
        {
            get
            {
                if (isDisposed)
                    throw new ObjectDisposedException(nameof(indexShard));

                return indexShard.DataPointsCount;
            }
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            indexShard.Dispose();
            isDisposed = true;
        }

        public void UpdateIndexShard(IReadOnlyList<DataPointOrTombstone<TVector>> dataPointOrTombstones)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(indexShard));

            indexShard.UpdateIndex(dataPointOrTombstones);
        }

        public ValidationResult ValidateSearchQuery(SearchQueryDto query)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(indexShard));

            return searchQueryValidator.Validate(query);
        }

        public SearchResultDto[] ExecuteSearchQuery(SearchQueryDto query)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(indexShard));

            var sw = Stopwatch.StartNew();

            var searchQuery = new SearchQuery<TVector>(
                query.SplitFilter?.ToDictionary(x => x.Key, x => x.Value.ToAttributeValue()),
                query.QueryVectors.Select(x => (TVector)x.ToVector(IndexMeta.VectorDimension)).ToArray(),
                query.K,
                query.RetrieveVectors);

            var searchResults = indexShard.FindNearest(searchQuery);

            var searchResultDtos = searchResults.Select(
                    x => new SearchResultDto(
                        x.QueryVector.ToVectorDto()!,
                        x.NearestDataPoints.Select(
                                p => new FoundDataPointDto(
                                    p.Vector.ToVectorDto(),
                                    p.Attributes.Select(t => new AttributeDto(t.Key, t.Value.ToAttributeValueDto())).ToArray(),
                                    p.Distance)
                            )
                            .ToArray())
                )
                .ToArray();

            log.Info($"Executing search query {{ {searchQuery} }} took {sw.Elapsed}");

            return searchResultDtos;
        }
    }
}
