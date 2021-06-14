using System;
using SpaceHosting.Index;

namespace SpaceHosting.Service
{
    public class IndexStoreHolder : IDisposable
    {
        public IndexStoreHolder(IIndexStore<int, string, DenseVector> indexStore, int vectorDimension, string indexDescription)
        {
            IndexStore = indexStore;
            VectorDimension = vectorDimension;
            IndexDescription = indexDescription;
        }

        public IIndexStore<int, string, DenseVector> IndexStore { get; }

        public int VectorDimension { get; }

        public string IndexDescription { get; }

        public void Dispose()
        {
            IndexStore.Dispose();
        }
    }
}
