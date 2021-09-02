using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SpaceHosting.Contracts.ApiModels;
using SpaceHosting.Service.IndexShard;

namespace SpaceHosting.Service.Controllers
{
    [ApiController]
    [Route("api/v1/[action]")]
    public class ApiController : ControllerBase
    {
        private readonly IIndexShardAccessor indexShardAccessor;

        public ApiController(IIndexShardAccessor indexShardAccessor)
        {
            this.indexShardAccessor = indexShardAccessor;
        }

        [HttpGet]
        public ActionResult<IndexInfoDto> Info()
        {
            return new IndexInfoDto(
                IndexAlgorithm: indexShardAccessor.IndexMeta.IndexAlgorithm,
                VectorType: indexShardAccessor.ZeroVector.GetType().Name,
                VectorDimension: indexShardAccessor.IndexMeta.VectorDimension,
                DataPointsCount: indexShardAccessor.DataPointsCount
            );
        }

        [HttpGet]
        public ActionResult<SearchResultDto> Probe(int? k)
        {
            var searchQuery = new SearchQueryDto(
                SplitFilter: null,
                QueryVectors: new[] {indexShardAccessor.ZeroVector},
                K: k ?? 1);

            if (searchQuery.K <= 0)
                return BadRequest(new {errorMessages = new[] {"searchQuery.K must be greater than 0"}});

            return indexShardAccessor.SearchQueryExecutor.ExecuteSearchQuery(searchQuery).Single();
        }

        [HttpGet]
        [HttpPost]
        public ActionResult<SearchResultDto[]> Search([FromBody] SearchQueryDto searchQuery)
        {
            var validationResult = indexShardAccessor.SearchQueryExecutor.ValidateSearchQuery(searchQuery);
            if (!validationResult.IsValid)
                return BadRequest(new {errorMessages = validationResult.Errors.Select(x => x.ErrorMessage).ToArray()});

            return indexShardAccessor.SearchQueryExecutor.ExecuteSearchQuery(searchQuery);
        }
    }
}
