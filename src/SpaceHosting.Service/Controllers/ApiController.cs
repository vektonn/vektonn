using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SpaceHosting.ApiModels;
using SpaceHosting.Service.IndexStore;

namespace SpaceHosting.Service.Controllers
{
    [ApiController]
    [Route("api/v1/[action]")]
    public class ApiController : ControllerBase
    {
        private readonly IIndexStoreAccessor indexStoreAccessor;

        public ApiController(IIndexStoreAccessor indexStoreAccessor)
        {
            this.indexStoreAccessor = indexStoreAccessor;
        }

        [HttpGet]
        public ActionResult<IndexInfoDto> Info()
        {
            return new IndexInfoDto
            {
                IndexAlgorithm = indexStoreAccessor.IndexAlgorithm,
                VectorType = indexStoreAccessor.ZeroVector.GetType().Name,
                VectorDimension = indexStoreAccessor.VectorDimension,
                VectorCount = indexStoreAccessor.VectorCount,
            };
        }

        [HttpGet]
        public ActionResult<SearchResultDto[]> Probe(int? k)
        {
            var searchQuery = new SearchQueryDto
            {
                Vectors = new[] {indexStoreAccessor.ZeroVector},
                K = k ?? 1
            };

            if (searchQuery.K <= 0)
                return BadRequest(new {message = "searchQuery.K must be greater than 0"});

            return indexStoreAccessor.Search(searchQuery).Single();
        }

        [HttpGet]
        [HttpPost]
        public ActionResult<SearchResultDto[][]> Search([FromBody] SearchQueryDto searchQuery)
        {
            if (searchQuery.K <= 0)
                return BadRequest(new {message = "searchQuery.K must be greater than 0"});

            if (!searchQuery.Vectors.Any())
                return BadRequest(new {message = "searchQuery.Vectors is empty"});

            if (searchQuery.Vectors.Any(queryVector => queryVector.Dimension != indexStoreAccessor.VectorDimension))
                return BadRequest(new {message = $"All searchQuery.Vectors must have the same dimension as index vectors: {indexStoreAccessor.VectorDimension}"});

            var indexVectorType = indexStoreAccessor.ZeroVector.GetType();
            if (searchQuery.Vectors.Any(queryVector => queryVector.GetType() != indexVectorType))
                return BadRequest(new {message = $"All searchQuery.Vectors must have the same type as index vectors: {indexVectorType}"});

            return indexStoreAccessor.Search(searchQuery);
        }
    }
}
