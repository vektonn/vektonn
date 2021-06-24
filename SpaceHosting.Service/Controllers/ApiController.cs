using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SpaceHosting.ApiModels;
using SpaceHosting.Index;

namespace SpaceHosting.Service.Controllers
{
    [Route("api/[action]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly IndexStoreHolder indexStoreHolder;

        public ApiController(IndexStoreHolder indexStoreHolder)
        {
            this.indexStoreHolder = indexStoreHolder;
        }

        public ActionResult<IndexInfoDto> Info()
        {
            return new IndexInfoDto
            {
                Description = indexStoreHolder.IndexDescription,
                VectorDimension = indexStoreHolder.VectorDimension,
                VectorCount = (int)indexStoreHolder.IndexStore.Count,
            };
        }

        [HttpGet]
        public SearchResultDto[] Probe(int? k)
        {
            var zeroVector = new DenseVector(new double[indexStoreHolder.VectorDimension]);
            var searchQuery = new SearchQueryDto
            {
                QueryVectors = new IVector[] {zeroVector},
                K = k ?? 1
            };

            return DoSearch(searchQuery).Single();
        }

        [HttpGet]
        [HttpPost]
        public SearchResultDto[][] Search([FromBody] SearchQueryDto searchQuery)
        {
            return DoSearch(searchQuery);
        }

        private SearchResultDto[][] DoSearch(SearchQueryDto searchQuery)
        {
            var queryDataPoints = searchQuery
                .QueryVectors
                .Cast<DenseVector>()
                .Select(vector => new IndexQueryDataPoint<DenseVector> {Vector = vector})
                .ToArray();

            var queryResults = indexStoreHolder.IndexStore.FindNearest(queryDataPoints, limitPerQuery: searchQuery.K);

            return queryResults.Select(
                    queryResult => queryResult.Nearest.Select(
                            foundDataPoint => new SearchResultDto
                            {
                                Distance = foundDataPoint.Distance,
                                Vector = foundDataPoint.Vector,
                                Data = foundDataPoint.Data
                            })
                        .ToArray())
                .ToArray();
        }
    }
}
