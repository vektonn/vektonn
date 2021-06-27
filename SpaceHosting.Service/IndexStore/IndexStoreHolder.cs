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
            ZeroVector = zeroVector.ToVectorDto();
            this.indexStore = indexStore;
        }

        public string IndexAlgorithm { get; }

        public int VectorDimension { get; }

        public VectorDto ZeroVector { get; }

        public int VectorCount => (int)indexStore.Count;

        public void Dispose()
        {
            indexStore.Dispose();
        }

        public SearchResultDto[][] Search(SearchQueryDto searchQuery)
        {
            var queryDataPoints = searchQuery
                .Vectors
                .Select(vector => new IndexQueryDataPoint<TVector> {Vector = (TVector)vector.ToVector()})
                .ToArray();

            var queryResults = indexStore.FindNearest(queryDataPoints, limitPerQuery: searchQuery.K);

            return queryResults.Select(
                    queryResult => queryResult.Nearest.Select(
                            foundDataPoint => new SearchResultDto
                            {
                                Distance = foundDataPoint.Distance,
                                Vector = foundDataPoint.Vector.ToVectorDto(),
                                Data = foundDataPoint.Data
                            })
                        .ToArray())
                .ToArray();
        }
    }
}
