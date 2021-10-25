using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Vektonn.SharedImpl.BinarySerialization;
using Vektonn.SharedImpl.Contracts;
using Vektonn.SharedImpl.Json;
using Vostok.Logging.Abstractions;

namespace Vektonn.DataSource.Kafka
{
    // todo (andrew, 25.09.2021): test
    public class KafkaDataSourceProducer : IDisposable, IDataSourceProducer
    {
        private readonly ConcurrentDictionary<DataSourceId, string[]> permanentAttributeKeysByDataSourceId = new();
        private readonly KafkaProducer kafkaProducer;

        public KafkaDataSourceProducer(ILog log, KafkaProducerConfig kafkaProducerConfig)
        {
            kafkaProducer = new KafkaProducer(log, kafkaProducerConfig);
        }

        public void Dispose()
        {
            kafkaProducer.Dispose();
        }

        public async Task ProduceAsync(DataSourceDescriptor dataSource, IReadOnlyList<InputDataPointOrTombstone> dataPointOrTombstones)
        {
            var produceTasksByTopic = dataPointOrTombstones
                .GroupBy(
                    x => GetTopicName(dataSource, x),
                    x => SerializeToKafkaMessage(dataSource, x))
                .Select(g => kafkaProducer.ProduceAsync(topicName: g.Key, messages: g.ToArray()));

            await Task.WhenAll(produceTasksByTopic);
        }

        private static string GetTopicName(DataSourceDescriptor dataSource, InputDataPointOrTombstone dataPointOrTombstone)
        {
            var attributes = dataPointOrTombstone.GetAttributes();
            var shardingCoordinatesByAttributeKey = dataSource.Meta.DataSourceShardingMeta.GetDataSourceShardingCoordinates(attributes);
            return KafkaTopicNameFormatter.FormatTopicName(dataSource.Id, shardingCoordinatesByAttributeKey);
        }

        private Message<byte[], byte[]> SerializeToKafkaMessage(DataSourceDescriptor dataSource, InputDataPointOrTombstone dataPointOrTombstone)
        {
            var permanentAttributes = GetPermanentAttributes(dataSource, dataPointOrTombstone);

            var kafkaMessage = new Message<byte[], byte[]>
            {
                Key = AttributeValueSerializer.Serialize(permanentAttributes),
                Timestamp = Timestamp.Default
            };

            if (dataPointOrTombstone.DataPoint != null)
                kafkaMessage.Value = InputDataPointSerializer.SerializeDataPoint(dataPointOrTombstone.DataPoint);

            return kafkaMessage;
        }

        private AttributeValue[] GetPermanentAttributes(DataSourceDescriptor dataSource, InputDataPointOrTombstone dataPointOrTombstone)
        {
            var permanentAttributeKeys = permanentAttributeKeysByDataSourceId.GetOrAdd(
                dataSource.Id,
                _ =>
                {
                    var permanentAttributes = dataSource.Meta.PermanentAttributes.OrderBy(x => x, StringComparer.InvariantCulture).ToArray();
                    if (!permanentAttributes.Any())
                        throw new InvalidOperationException($"{nameof(permanentAttributes)} is empty for dataSource: {dataSource.ToPrettyJson()}");

                    return permanentAttributes;
                });

            var attributeValues = dataPointOrTombstone.GetAttributes();

            return permanentAttributeKeys.Select(key => attributeValues[key]).ToArray();
        }
    }
}
