using System;
using System.Linq;
using SpaceHosting.ApiModels;
using SpaceHosting.Index;

namespace SpaceHosting.Service.IndexStore
{
    public class IndexStoreHolder<TVector> : IDisposable, IIndexStoreAccessor
        where TVector : IVector
    {
        private readonly IIndexStore<int, object, TVector> indexStore;

        public IndexStoreHolder(
            string indexAlgorithm,
            int vectorDimension,
            TVector zeroVector,
            IIndexStore<int, object, TVector> indexStore)
        {
            IndexAlgorithm = indexAlgorithm;
            VectorDimension = vectorDimension;
            ZeroVector = zeroVector;
            this.indexStore = indexStore;
        }

        public string IndexAlgorithm { get; }

        public int VectorDimension { get; }

        public IVector ZeroVector { get; }

        public int VectorCount => (int)indexStore.Count;

        public void Dispose()
        {
            indexStore.Dispose();
        }

        public SearchResultDto[][] Search(SearchQueryDto searchQuery)
        {
            var queryDataPoints = searchQuery
                .Vectors
                .Cast<TVector>()
                .Select(vector => new IndexQueryDataPoint<TVector> {Vector = vector})
                .ToArray();

            var queryResults = indexStore.FindNearest(queryDataPoints, limitPerQuery: searchQuery.K);

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
