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
            var vectorsFileName = EnvironmentVariables.Get("SH_VECTORS_FILE_NAME");
            var vectorsFileFormat = EnvironmentVariables.TryGet("SH_VECTORS_FILE_FORMAT", Enum.Parse<VectorsFileFormat>, defaultValue: VectorsFileFormat.VectorArrayJson);
            var metadataFileName = EnvironmentVariables.TryGet("SH_VECTORS_METADATA_FILE_NAME");
            var algorithm = EnvironmentVariables.Get("SH_INDEX_ALGORITHM");
            var indexBatchSize = EnvironmentVariables.TryGet("SH_INDEX_BATCH_SIZE", int.Parse, defaultValue: 1000);

            var indexDataPoints = ReadAllDataPoints(vectorsFileName, vectorsFileFormat, metadataFileName);
            var vectorDimension = indexDataPoints.First().Vector.Dimension;
            var indexStore = new IndexStoreFactory<int, object>(log)
                .Create<DenseVector>(algorithm, vectorDimension, withDataStorage: metadataFileName != null, idComparer: EqualityComparer<int>.Default);

            foreach (var batch in indexDataPoints.Batch(indexBatchSize, b => b.ToArray()))
                indexStore.AddBatch(batch);

            return new IndexStoreHolder(
                indexStore,
                vectorDimension,
                indexDescription: $"{algorithm} index with VectorDimension: {vectorDimension}");
        }

        private static IList<IndexDataPoint<int, object, DenseVector>> ReadAllDataPoints(string vectorsFileName, VectorsFileFormat vectorsFileFormat, string? metadataFileName)
        {
            var vectors = vectorsFileFormat switch
            {
                VectorsFileFormat.VectorArrayJson => ReadVectorArrayFile(vectorsFileName),
                VectorsFileFormat.PandasDataFrameCsv => ReadPandasDataFrameCsvFile(vectorsFileName),
                VectorsFileFormat.PandasDataFrameJson => ReadPandasDataFrameJsonFile(vectorsFileName),
                _ => throw new ArgumentException($"Invalid vectorsFileFormat: {vectorsFileFormat}")
            };

            var vectorDimension = vectors.First().Length;
            if (vectors.Any(vector => vector.Length != vectorDimension))
                throw new InvalidOperationException("All vectors must have the same dimension");

            object[]? metadata = null;
            if (metadataFileName != null)
            {
                var json = File.ReadAllText(metadataFileName);
                metadata = JsonSerializer.Deserialize<object[]>(json)!;

                if (metadata.Length != vectors.Count)
                    throw new InvalidOperationException("All vectors must have corresponding metadata");
            }

            return vectors
                .Select(
                    (v, i) => new IndexDataPoint<int, object, DenseVector>
                    {
                        Id = i,
                        Vector = new DenseVector(v),
                        Data = metadata?[i],
                    })
                .ToArray();
        }

        private static List<double[]> ReadVectorArrayFile(string vectorsFileName)
        {
            var json = File.ReadAllText(vectorsFileName);
            return JsonSerializer.Deserialize<List<double[]>>(json)!;
        }

        private static List<double[]> ReadPandasDataFrameCsvFile(string vectorsFileName)
        {
            return File
                .ReadAllLines(vectorsFileName)
                .Select(line => line.Split(',').Select(double.Parse).ToArray())
                .ToList();
        }

        private static List<double[]> ReadPandasDataFrameJsonFile(string vectorsFileName)
        {
            return File
                .ReadAllLines(vectorsFileName)
                .Select(
                    line =>
                    {
                        var record = JsonSerializer.Deserialize<Dictionary<int, double>>(line)!;
                        var vector = record.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToArray();
                        return vector;
                    })
                .ToList();
        }
    }
}
