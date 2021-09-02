using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using MoreLinq;
using SpaceHosting.Index;
using Vostok.Logging.Abstractions;

namespace SpaceHosting.Service.IndexStore
{
    public class IndexStoreBuilder
    {
        private readonly ILog log;

        public IndexStoreBuilder(ILog log)
        {
            this.log = log;
        }

        public IIndexStoreAccessor BuildIndexStore()
        {
            var vectorsFileName = EnvironmentVariables.Get("SH_VECTORS_FILE_NAME");
            var vectorsFileFormat = EnvironmentVariables.TryGet("SH_VECTORS_FILE_FORMAT", Enum.Parse<VectorsFileFormat>, defaultValue: VectorsFileFormat.VectorArrayJson);
            var metadataFileName = EnvironmentVariables.TryGet("SH_VECTORS_METADATA_FILE_NAME");
            var indexAlgorithm = EnvironmentVariables.Get("SH_INDEX_ALGORITHM");
            var indexBatchSize = EnvironmentVariables.TryGet("SH_INDEX_BATCH_SIZE", int.Parse, defaultValue: 1000);

            var vectors = ReadVectors(vectorsFileName, vectorsFileFormat);
            var metadata = metadataFileName == null ? null : ReadMetadata(metadataFileName, vectors.Count);

            if (indexAlgorithm.StartsWith(Algorithms.FaissIndex))
                return BuildIndexStore(vectors, metadata, indexAlgorithm, indexBatchSize, CreateDenseVector);

            if (indexAlgorithm.StartsWith(Algorithms.SparnnIndex))
                return BuildIndexStore(vectors, metadata, indexAlgorithm, indexBatchSize, CreateSparseVector);

            throw new ArgumentException($"Invalid indexAlgorithm: {indexAlgorithm}");
        }

        private static DenseVector CreateDenseVector(double?[] coordinates)
        {
            if (coordinates.All(c => c == null))
                return new DenseVector(new double[coordinates.Length]);

            return new DenseVector(coordinates.Cast<double>().ToArray());
        }

        private static SparseVector CreateSparseVector(double?[] coordinates)
        {
            var dimension = coordinates.Length;
            var nonNullCount = coordinates.Count(c => c != null);
            var indices = new int[nonNullCount];
            var coords = new double[nonNullCount];

            var j = 0;
            for (var i = 0; i < dimension; i++)
            {
                var coordinate = coordinates[i];
                if (coordinate == null)
                    continue;

                indices[j] = i;
                coords[j] = coordinate.Value;
                j++;
            }

            return new SparseVector(dimension, coords, indices);
        }

        private IIndexStoreAccessor BuildIndexStore<TVector>(
            List<double?[]> vectors,
            object[]? metadata,
            string indexAlgorithm,
            int indexBatchSize,
            Func<double?[], TVector> createVector)
            where TVector : IVector
        {
            var indexDataPoints = vectors
                .Select(
                    (v, i) => new IndexDataPointOrTombstone<int, object, TVector>(
                        new IndexDataPoint<int, object, TVector>(
                            Id: i,
                            Vector: createVector(v),
                            Data: metadata?[i]))
                )
                .ToArray();

            var vectorDimension = indexDataPoints.First().DataPoint!.Vector.Dimension;
            var indexStore = new IndexStoreFactory<int, object>(log).Create<TVector>(
                indexAlgorithm,
                vectorDimension,
                withDataStorage: metadata != null,
                idComparer: EqualityComparer<int>.Default);

            foreach (var batch in indexDataPoints.Batch(indexBatchSize, b => b.ToArray()))
                indexStore.UpdateIndex(batch);

            var zeroVector = createVector(new double?[vectorDimension]);
            return new IndexStoreHolder<TVector>(indexAlgorithm, vectorDimension, zeroVector, indexStore);
        }

        private static object[] ReadMetadata(string metadataFileName, int vectorCount)
        {
            var json = File.ReadAllText(metadataFileName);
            var metadata = JsonSerializer.Deserialize<object[]>(json)!;

            if (metadata.Length != vectorCount)
                throw new InvalidOperationException("All vectors must have corresponding metadata");

            return metadata;
        }

        private static List<double?[]> ReadVectors(string vectorsFileName, VectorsFileFormat vectorsFileFormat)
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

            return vectors;
        }

        private static List<double?[]> ReadVectorArrayFile(string vectorsFileName)
        {
            var json = File.ReadAllText(vectorsFileName);
            return JsonSerializer.Deserialize<List<double?[]>>(json)!;
        }

        private static List<double?[]> ReadPandasDataFrameCsvFile(string vectorsFileName)
        {
            return File
                .ReadAllLines(vectorsFileName)
                .Select(line => line.Split(',').Select(x => string.IsNullOrEmpty(x) ? (double?)null : double.Parse(x, CultureInfo.InvariantCulture)).ToArray())
                .ToList();
        }

        private static List<double?[]> ReadPandasDataFrameJsonFile(string vectorsFileName)
        {
            return File
                .ReadAllLines(vectorsFileName)
                .Select(
                    line =>
                    {
                        var record = JsonSerializer.Deserialize<Dictionary<int, double?>>(line)!;
                        var vector = record.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToArray();
                        return vector;
                    })
                .ToList();
        }
    }
}
