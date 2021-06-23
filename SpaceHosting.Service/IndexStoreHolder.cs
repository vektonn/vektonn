using System;
using SpaceHosting.Index;

namespace SpaceHosting.Service
{
    public class IndexStoreHolder : IDisposable
    {
        public IndexStoreHolder(IIndexStore<int, object, DenseVector> indexStore, int vectorDimension, string indexDescription)
        {
            IndexStore = indexStore;
            VectorDimension = vectorDimension;
            IndexDescription = indexDescription;
        }

        public IIndexStore<int, object, DenseVector> IndexStore { get; }

        public int VectorDimension { get; }

        public string IndexDescription { get; }

        public void Dispose()
        {
            IndexStore.Dispose();
        }
    }
}
