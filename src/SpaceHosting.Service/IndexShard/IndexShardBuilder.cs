using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using MoreLinq;
using SpaceHosting.Contracts;
using SpaceHosting.Contracts.Sharding.DataSource;
using SpaceHosting.Contracts.Sharding.Index;
using SpaceHosting.Index;
using SpaceHosting.IndexShard.Shard;
using Vostok.Logging.Abstractions;

namespace SpaceHosting.Service.IndexShard
{
    public class IndexShardBuilder
    {
        private const string IdAttributeKey = "Id";

        private readonly ILog log;

        public IndexShardBuilder(ILog log)
        {
            this.log = log;
        }

        public IIndexShardAccessor BuildIndexShard()
        {
            var vectorsFileName = EnvironmentVariables.Get("SH_VECTORS_FILE_NAME");
            var vectorsFileFormat = EnvironmentVariables.TryGet("SH_VECTORS_FILE_FORMAT", Enum.Parse<VectorsFileFormat>, defaultValue: VectorsFileFormat.VectorArrayJson);
            var metadataFileName = EnvironmentVariables.TryGet("SH_VECTORS_METADATA_FILE_NAME");
            var indexAlgorithm = EnvironmentVariables.Get("SH_INDEX_ALGORITHM");
            var indexBatchSize = EnvironmentVariables.TryGet("SH_INDEX_BATCH_SIZE", int.Parse, defaultValue: 1000);

            var vectors = ReadVectors(vectorsFileName, vectorsFileFormat);
            var metadata = metadataFileName == null ? null : ReadMetadata(metadataFileName, vectors.Count);

            if (indexAlgorithm.StartsWith(Algorithms.FaissIndex))
                return BuildIndexShard(vectors, metadata, indexAlgorithm, indexBatchSize, CreateDenseVector);

            if (indexAlgorithm.StartsWith(Algorithms.SparnnIndex))
                return BuildIndexShard(vectors, metadata, indexAlgorithm, indexBatchSize, CreateSparseVector);

            throw new InvalidOperationException($"Invalid indexAlgorithm: {indexAlgorithm}");
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

        private IIndexShardAccessor BuildIndexShard<TVector>(
            List<double?[]> rawVectors,
            object[]? metadata,
            string indexAlgorithm,
            int indexBatchSize,
            Func<double?[], TVector> createVector)
            where TVector : IVector
        {
            var vectors = rawVectors.Select(createVector).ToArray();
            var vectorDimension = vectors.First().Dimension;

            var indexMeta = BuildIndexMeta<TVector>(indexAlgorithm, vectorDimension);
            var indexShardHolder = new IndexShardHolder<TVector>(log, indexMeta);

            var dataPoints = vectors
                .Select(
                    (v, i) => new DataPointOrTombstone<TVector>(
                        new DataPoint<TVector>(
                            Vector: v,
                            Attributes: new Dictionary<string, AttributeValue>
                            {
                                {IdAttributeKey, new AttributeValue(Int64: i)}
                            }))
                )
                .ToArray();

            foreach (var batch in dataPoints.Batch(indexBatchSize, b => b.ToArray()))
                indexShardHolder.UpdateIndexShard(batch);

            var zeroVector = createVector(new double?[indexMeta.VectorDimension]);
            return new IndexShardAccessor<TVector>(indexShardHolder, zeroVector);
        }

        private static IndexMeta BuildIndexMeta<TVector>(string indexAlgorithm, int vectorDimension)
            where TVector : IVector
        {
            var indexMeta = new IndexMeta(
                DataSourceMeta: new DataSourceMeta(
                    vectorDimension,
                    VectorsAreSparse: typeof(TVector) == typeof(SparseVector),
                    IdAttributes: new HashSet<string> {IdAttributeKey},
                    DataSourceShardingMeta: new DataSourceShardingMeta(new Dictionary<string, IDataSourceAttributeValueSharder>()),
                    AttributeValueTypes: new Dictionary<string, AttributeValueTypeCode>
                    {
                        {IdAttributeKey, AttributeValueTypeCode.Int64}
                    }),
                IndexAlgorithm: indexAlgorithm,
                SplitAttributes: new HashSet<string>(),
                IndexShardsMap: new IndexShardsMapMeta(new Dictionary<string, IndexShardMeta>())
            );

            indexMeta.ValidateConsistency();

            return indexMeta;
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
                _ => throw new InvalidOperationException($"Invalid vectorsFileFormat: {vectorsFileFormat}")
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
