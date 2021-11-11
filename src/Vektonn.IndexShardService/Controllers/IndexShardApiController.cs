using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Vektonn.ApiContracts;
using Vektonn.IndexShardService.Services;

namespace Vektonn.IndexShardService.Controllers
{
    [ApiController]
    [Route("api/v1/[action]")]
    public class IndexShardApiController : ControllerBase
    {
        private readonly IIndexShardAccessor indexShardAccessor;

        public IndexShardApiController(IIndexShardAccessor indexShardAccessor)
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
                return BadRequest(new ErrorDto(ErrorMessages: new[] {"searchQuery.K must be greater than 0"}));

            return indexShardAccessor.SearchQueryExecutor.ExecuteSearchQuery(searchQuery).Single();
        }

        [HttpGet]
        [HttpPost]
        public ActionResult<SearchResultDto[]> Search([FromBody] SearchQueryDto searchQuery)
        {
            var validationResult = indexShardAccessor.SearchQueryExecutor.ValidateSearchQuery(searchQuery);
            if (!validationResult.IsValid)
                return BadRequest(new ErrorDto(ErrorMessages: validationResult.Errors.Select(x => x.ErrorMessage).ToArray()));

            return indexShardAccessor.SearchQueryExecutor.ExecuteSearchQuery(searchQuery);
        }
    }
}
