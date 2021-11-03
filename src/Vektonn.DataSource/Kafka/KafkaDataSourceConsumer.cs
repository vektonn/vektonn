using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Vektonn.Index;
using Vektonn.SharedImpl.BinarySerialization;
using Vektonn.SharedImpl.Contracts;
using Vektonn.SharedImpl.Contracts.Sharding.Index;
using Vostok.Logging.Abstractions;

namespace Vektonn.DataSource.Kafka
{
    // todo (andrew, 25.09.2021): test
    public class KafkaDataSourceConsumer<TVector> : IDisposable, IDataSourceConsumer
        where TVector : IVector
    {
        private readonly IndexShardMeta indexShardMeta;
        private readonly Action<IReadOnlyList<DataPointOrTombstone<TVector>>> updateIndexShard;
        private readonly string[] permanentAttributeKeys;
        private readonly Func<InputDataPoint, TVector> decodeVector;
        private readonly KafkaConsumer kafkaConsumer;
        private bool isDisposed;

        public KafkaDataSourceConsumer(
            ILog log,
            KafkaConsumerConfig kafkaConsumerConfig,
            DataSourceMeta dataSourceMeta,
            IndexShardMeta indexShardMeta,
            Action<IReadOnlyList<DataPointOrTombstone<TVector>>> updateIndexShard)
        {
            this.indexShardMeta = indexShardMeta;
            this.updateIndexShard = updateIndexShard;

            permanentAttributeKeys = dataSourceMeta.PermanentAttributes.OrderBy(x => x, StringComparer.InvariantCulture).ToArray();

            decodeVector = dataSourceMeta.VectorsAreSparse
                ? inputDataPoint => (TVector)(IVector)new SparseVector(dataSourceMeta.VectorDimension, inputDataPoint.VectorCoordinates, inputDataPoint.VectorCoordinateIndices!)
                : inputDataPoint => (TVector)(IVector)new DenseVector(inputDataPoint.VectorCoordinates);

            var topicsToConsume = indexShardMeta.DataSourceShardsToConsume
                .Select(x => KafkaTopicNameFormatter.FormatTopicNameOrPattern(dataSourceMeta.Id, x.ShardingCoordinatesByAttributeKey))
                .ToArray();

            kafkaConsumer = new KafkaConsumer(log, kafkaConsumerConfig, topicsToConsume, ProcessKafkaMessages);
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            kafkaConsumer.Dispose();
            isDisposed = true;
        }

        public Task RunAsync(CancellationToken cancellationToken)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(kafkaConsumer));

            return kafkaConsumer.RunAsync(cancellationToken);
        }

        private void ProcessKafkaMessages(IReadOnlyList<Message<byte[], byte[]>> kafkaMessages)
        {
            var dataPointOrTombstones = new List<DataPointOrTombstone<TVector>>();

            foreach (var kafkaMessage in kafkaMessages)
            {
                var permanentAttributes = GetPermanentAttributes(kafkaMessage.Key);

                if (!indexShardMeta.Contains(permanentAttributes))
                    continue;

                if (kafkaMessage.Value == null)
                    dataPointOrTombstones.Add(new DataPointOrTombstone<TVector>(new Tombstone(permanentAttributes)));
                else
                {
                    var dataPoint = GetDataPoint(kafkaMessage.Value);
                    dataPointOrTombstones.Add(new DataPointOrTombstone<TVector>(dataPoint));
                }
            }

            updateIndexShard(dataPointOrTombstones);
        }

        private Dictionary<string, AttributeValue> GetPermanentAttributes(byte[] kafkaMessageKey)
        {
            var permanentAttributeValues = AttributeValueSerializer.Deserialize(kafkaMessageKey);
            if (permanentAttributeValues.Length != permanentAttributeKeys.Length)
                throw new InvalidOperationException($"values.Length ({permanentAttributeValues.Length}) != keys.Length {permanentAttributeKeys.Length}");

            return permanentAttributeKeys.Zip(permanentAttributeValues).ToDictionary(t => t.First, t => t.Second);
        }

        private DataPoint<TVector> GetDataPoint(byte[] kafkaMessageValue)
        {
            var inputDataPoint = InputDataPointSerializer.DeserializeDataPoint(kafkaMessageValue);
            var vector = decodeVector(inputDataPoint);
            return new DataPoint<TVector>(vector, inputDataPoint.Attributes);
        }
    }
}
