using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SpaceHosting.Index;

namespace SpaceHosting.Service.Controllers
{
    [Route("/")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly IndexStoreHolder indexStoreHolder;

        public ApiController(IndexStoreHolder indexStoreHolder)
        {
            this.indexStoreHolder = indexStoreHolder;
        }

        public ActionResult<object> Index()
        {
            return new
            {
                Description = indexStoreHolder.IndexDescription,
                VectorCount = indexStoreHolder.IndexStore.Count
            };
        }

        [HttpGet("search")]
        public IndexFoundDataPoint<int, string, DenseVector>[] Search(int? k)
        {
            var queryDataPoint = new IndexQueryDataPoint<DenseVector> {Vector = new DenseVector(new double[indexStoreHolder.VectorDimension])};

            var queryResults = indexStoreHolder.IndexStore.FindNearest(new[] {queryDataPoint}, limitPerQuery: k ?? 1);

            return queryResults.Single().Nearest;
        }
    }
}
