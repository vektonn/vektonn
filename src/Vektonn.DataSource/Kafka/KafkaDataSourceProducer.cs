using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Vektonn.SharedImpl.BinarySerialization;
using Vektonn.SharedImpl.Contracts;
using Vostok.Logging.Abstractions;

namespace Vektonn.DataSource.Kafka
{
    // todo (andrew, 25.09.2021): test
    public class KafkaDataSourceProducer : IDisposable, IDataSourceProducer
    {
        private readonly ConcurrentDictionary<DataSourceId, string[]> permanentAttributeKeysByDataSourceId = new();
        private readonly KafkaProducer kafkaProducer;
        private bool isDisposed;

        public KafkaDataSourceProducer(ILog log, KafkaProducerConfig kafkaProducerConfig)
        {
            kafkaProducer = new KafkaProducer(log, kafkaProducerConfig);
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            kafkaProducer.Dispose();
            isDisposed = true;
        }

        public async Task ProduceAsync(DataSourceMeta dataSourceMeta, IReadOnlyList<InputDataPointOrTombstone> dataPointOrTombstones)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(kafkaProducer));

            var produceTasksByTopic = dataPointOrTombstones
                .GroupBy(
                    x => GetTopicName(dataSourceMeta, x),
                    x => SerializeToKafkaMessage(dataSourceMeta, x))
                .Select(g => kafkaProducer.ProduceAsync(topicName: g.Key, messages: g.ToArray()));

            await Task.WhenAll(produceTasksByTopic);
        }

        private static string GetTopicName(DataSourceMeta dataSourceMeta, InputDataPointOrTombstone dataPointOrTombstone)
        {
            var attributes = dataPointOrTombstone.GetAttributes();
            var shardingCoordinatesByAttributeKey = dataSourceMeta.DataSourceShardingMeta.GetDataSourceShardingCoordinates(attributes);
            return KafkaTopicNameFormatter.FormatTopicName(dataSourceMeta.Id, shardingCoordinatesByAttributeKey);
        }

        private Message<byte[], byte[]> SerializeToKafkaMessage(DataSourceMeta dataSourceMeta, InputDataPointOrTombstone dataPointOrTombstone)
        {
            var permanentAttributes = GetPermanentAttributes(dataSourceMeta, dataPointOrTombstone);

            var kafkaMessage = new Message<byte[], byte[]>
            {
                Key = AttributeValueSerializer.Serialize(permanentAttributes),
                Timestamp = Timestamp.Default
            };

            if (dataPointOrTombstone.DataPoint != null)
                kafkaMessage.Value = InputDataPointSerializer.SerializeDataPoint(dataPointOrTombstone.DataPoint);

            return kafkaMessage;
        }

        private AttributeValue[] GetPermanentAttributes(DataSourceMeta dataSourceMeta, InputDataPointOrTombstone dataPointOrTombstone)
        {
            var permanentAttributeKeys = permanentAttributeKeysByDataSourceId.GetOrAdd(
                dataSourceMeta.Id,
                _ => dataSourceMeta.GetPermanentAttributeKeysOrdered());

            var attributeValues = dataPointOrTombstone.GetAttributes();

            return permanentAttributeKeys.Select(key => attributeValues[key]).ToArray();
        }
    }
}
