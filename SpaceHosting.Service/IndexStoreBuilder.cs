using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MoreLinq;
using SpaceHosting.Index;
using Vostok.Logging.Abstractions;

namespace SpaceHosting.Service
{
    public class IndexStoreBuilder
    {
        private readonly ILog log;

        public IndexStoreBuilder(ILog log)
        {
            this.log = log;
        }

        public IndexStoreHolder BuildIndexStore()
        {
            var vectorsFileName = EnvironmentVariables.Get("SH_VECTORS_FILENAME");
            var algorithm = EnvironmentVariables.Get("SH_INDEX_ALGORITHM");
            var indexBatchSize = EnvironmentVariables.TryGet("SH_INDEX_BATCH_SIZE", int.Parse, defaultValue: 1000);

            var indexDataPoints = ReadAllDataPoints(vectorsFileName);
            var vectorDimension = indexDataPoints.First().Vector.Dimension;
            var indexStore = new IndexStoreFactory<int, string>(log)
                .Create<DenseVector>(algorithm, vectorDimension, withDataStorage: false, EqualityComparer<int>.Default);

            foreach (var batch in indexDataPoints.Batch(indexBatchSize, b => b.ToArray()))
                indexStore.AddBatch(batch);

            return new IndexStoreHolder(
                indexStore,
                vectorDimension,
                indexDescription: $"{algorithm} index with VectorDimension: {vectorDimension}");
        }

        private static IList<IndexDataPoint<int, string, DenseVector>> ReadAllDataPoints(string vectorsFileName)
        {
            var json = File.ReadAllText(vectorsFileName);

            var vectors = JsonSerializer.Deserialize<List<double[]>>(json)!;

            var vectorDimension = vectors.First().Length;
            if (vectors.Any(vector => vector.Length != vectorDimension))
                throw new InvalidOperationException("All vectors must have the same dimension");

            return vectors
                .Select(
                    (v, i) => new IndexDataPoint<int, string, DenseVector>
                    {
                        Id = i,
                        Vector = new DenseVector(v),
                        Data = null,
                    })
                .ToArray();
        }
    }
}
