using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Vektonn.Contracts;
using Vektonn.Contracts.Sharding.Index;
using Vektonn.Index;
using Vektonn.SharedImpl.BinarySerialization;
using Vostok.Logging.Abstractions;

namespace Vektonn.DataSource.Kafka
{
    // todo (andrew, 25.09.2021): test
    public class KafkaDataSourceConsumer<TVector> : IDisposable, IDataSourceConsumer
        where TVector : IVector
    {
        private readonly IndexShardMeta indexShardMeta;
        private readonly Action<IReadOnlyList<DataPointOrTombstone<TVector>>> updateIndexShard;
        private readonly string[] idAttributeKeys;
        private readonly Func<InputDataPoint, TVector> decodeVector;
        private readonly KafkaConsumer kafkaConsumer;

        public KafkaDataSourceConsumer(
            ILog log,
            KafkaConsumerConfig kafkaConsumerConfig,
            DataSourceDescriptor dataSource,
            IndexShardMeta indexShardMeta,
            Action<IReadOnlyList<DataPointOrTombstone<TVector>>> updateIndexShard)
        {
            this.indexShardMeta = indexShardMeta;
            this.updateIndexShard = updateIndexShard;

            idAttributeKeys = dataSource.Meta.IdAttributes.OrderBy(x => x, StringComparer.InvariantCulture).ToArray();

            decodeVector = dataSource.Meta.VectorsAreSparse
                ? inputDataPoint => (TVector)(IVector)new SparseVector(dataSource.Meta.VectorDimension, inputDataPoint.VectorCoordinates, inputDataPoint.VectorCoordinateIndices!)
                : inputDataPoint => (TVector)(IVector)new DenseVector(inputDataPoint.VectorCoordinates);

            var topicsToConsume = indexShardMeta.DataSourceShardsToConsume
                .Select(x => KafkaTopicNameFormatter.FormatTopicNameOrPattern(dataSource.Id, x.ShardingCoordinatesByAttributeKey))
                .ToArray();

            kafkaConsumer = new KafkaConsumer(log, kafkaConsumerConfig, topicsToConsume, ProcessKafkaMessages);
        }

        public void Dispose()
        {
            kafkaConsumer.Dispose();
        }

        public Task RunAsync(CancellationToken cancellationToken)
        {
            return kafkaConsumer.RunAsync(cancellationToken);
        }

        private void ProcessKafkaMessages(IReadOnlyList<Message<byte[], byte[]>> kafkaMessages)
        {
            var dataPointOrTombstones = new List<DataPointOrTombstone<TVector>>();

            foreach (var kafkaMessage in kafkaMessages)
            {
                var idAttributes = GetIdAttributes(kafkaMessage.Key);

                if (!indexShardMeta.Contains(idAttributes))
                    continue;

                if (kafkaMessage.Value == null)
                    dataPointOrTombstones.Add(new DataPointOrTombstone<TVector>(new Tombstone(idAttributes)));
                else
                {
                    var dataPoint = GetDataPoint(kafkaMessage.Value);
                    dataPointOrTombstones.Add(new DataPointOrTombstone<TVector>(dataPoint));
                }
            }

            updateIndexShard(dataPointOrTombstones);
        }

        private Dictionary<string, AttributeValue> GetIdAttributes(byte[] kafkaMessageKey)
        {
            var idAttributeValues = AttributeValueSerializer.Deserialize(kafkaMessageKey);
            if (idAttributeValues.Length != idAttributeKeys.Length)
                throw new InvalidOperationException($"values.Length ({idAttributeValues.Length}) != keys.Length {idAttributeKeys.Length}");

            return idAttributeKeys.Zip(idAttributeValues).ToDictionary(t => t.First, t => t.Second);
        }

        private DataPoint<TVector> GetDataPoint(byte[] kafkaMessageValue)
        {
            var inputDataPoint = InputDataPointSerializer.DeserializeDataPoint(kafkaMessageValue);
            var vector = decodeVector(inputDataPoint);
            return new DataPoint<TVector>(vector, inputDataPoint.Attributes);
        }
    }
}
