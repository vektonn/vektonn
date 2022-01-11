using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vektonn.ApiContracts;
using Vektonn.ApiService.Services;
using Vektonn.Hosting;
using Vektonn.Index;
using Vektonn.SharedImpl.ApiContracts;
using Vektonn.SharedImpl.ApiContracts.Validation;
using Vektonn.SharedImpl.Configuration;
using Vektonn.SharedImpl.Contracts;
using Vektonn.SharedImpl.SearchResultsMerging;
using Vostok.Logging.Abstractions;

namespace Vektonn.ApiService.Controllers
{
    [ApiController]
    [Route("api/v1/search/{indexName}/{indexVersion}")]
    public class SearchApiController : ControllerBase
    {
        private static readonly ByPropertyComparer<FoundDataPointDto, double> FoundDataPointsComparer = new(x => x.Distance);

        private readonly ILog log;
        private readonly IShutdownTokenProvider shutdownTokenProvider;
        private readonly IIndexMetaProvider indexMetaProvider;
        private readonly IndexShardApiClientProvider indexShardApiClientProvider;

        public SearchApiController(
            ILog log,
            IShutdownTokenProvider shutdownTokenProvider,
            IIndexMetaProvider indexMetaProvider,
            IndexShardApiClientProvider indexShardApiClientProvider)
        {
            this.log = log;
            this.shutdownTokenProvider = shutdownTokenProvider;
            this.indexMetaProvider = indexMetaProvider;
            this.indexShardApiClientProvider = indexShardApiClientProvider;
        }

        [HttpGet]
        [HttpPost]
        public async Task<ActionResult<SearchResultDto[]>> Search([Required] string indexName, [Required] string indexVersion, [FromBody] SearchQueryDto searchQuery)
        {
            var indexId = new IndexId(indexName, indexVersion);

            var indexMetaWithShardEndpoints = indexMetaProvider.TryGetIndexMeta(indexId);
            if (indexMetaWithShardEndpoints == null)
                return NotFound(new ErrorDto(ErrorMessages: new[] {$"Index {indexId} does not exist"}));

            var (indexMeta, endpointsByShardId) = indexMetaWithShardEndpoints;

            var searchQueryValidator = new SearchQueryValidator(indexMeta);
            var validationResult = await searchQueryValidator.ValidateAsync(searchQuery);
            if (!validationResult.IsValid)
                return BadRequest(new ErrorDto(ErrorMessages: validationResult.Errors.Select(x => x.ErrorMessage).ToArray()));

            var splitFilter = searchQuery.SplitFilter?.Select(t => (AttributeKey: t.Key, t.Value.ToAttributeValue())).ToArray();
            var shardIdsForQuery = indexMeta.IndexShardsMap.GetShardIdsForQuery(splitFilter);

            var logMessage = $"Using {shardIdsForQuery.Count}/{indexMeta.IndexShardsMap.TotalShardsCount} shards to execute search query: {searchQuery}";
            if (shardIdsForQuery.Count == 0)
                log.Warn(logMessage);
            else
                log.Info(logMessage);

            switch (shardIdsForQuery.Count)
            {
                case 0:
                {
                    return searchQuery.QueryVectors
                        .Select(queryVector => new SearchResultDto(queryVector, NearestDataPoints: Array.Empty<FoundDataPointDto>()))
                        .ToArray();
                }
                case 1:
                {
                    return await QueryShardAsync(endpointsByShardId, shardIdsForQuery.Single(), searchQuery);
                }
                default:
                {
                    var tasks = shardIdsForQuery
                        .Select(shardId => QueryShardAsync(endpointsByShardId, shardId, searchQuery))
                        .ToArray();
                    var results = await Task.WhenAll(tasks);

                    var mergeSortDirection = AlgorithmTraits.GetMergeSortDirection(indexMeta.IndexAlgorithm.Type);
                    return MergeResultsOfShards(searchQuery, results, mergeSortDirection);
                }
            }
        }

        private async Task<SearchResultDto[]> QueryShardAsync(Dictionary<string, DnsEndPoint> endpointsByShardId, string shardId, SearchQueryDto searchQuery)
        {
            var indexShardApiClient = indexShardApiClientProvider.GetIndexShardApiClient(endpointsByShardId, shardId);

            return await indexShardApiClient.SearchAsync(searchQuery, shutdownTokenProvider.HostShutdownToken);
        }

        private static SearchResultDto[] MergeResultsOfShards(SearchQueryDto searchQuery, SearchResultDto[][] results, ListSortDirection mergeSortDirection)
        {
            var nearestDataPointsInShards = results.Select(r => r.Select(x => x.NearestDataPoints).ToArray());
            var nearestDataPointsPerVector = FoundDataPointsMerger.Merge(nearestDataPointsInShards, searchQuery.K, mergeSortDirection, FoundDataPointsComparer);

            return searchQuery.QueryVectors
                .Zip(
                    nearestDataPointsPerVector,
                    (queryVector, nearestDataPoints) => new SearchResultDto(queryVector, nearestDataPoints))
                .ToArray();
        }
    }
}
