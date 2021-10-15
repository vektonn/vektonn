using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Vektonn.Contracts;
using Vektonn.SharedImpl.BinarySerialization;
using Vektonn.SharedImpl.Json;
using Vostok.Logging.Abstractions;

namespace Vektonn.DataSource.Kafka
{
    // todo (andrew, 25.09.2021): test
    public class KafkaDataSourceProducer : IDisposable, IDataSourceProducer
    {
        private readonly ConcurrentDictionary<DataSourceId, string[]> idAttributeKeysByDataSourceId = new();
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
            var dataPointId = GetDataPointId(dataSource, dataPointOrTombstone);

            var kafkaMessage = new Message<byte[], byte[]>
            {
                Key = AttributeValueSerializer.Serialize(dataPointId),
                Timestamp = Timestamp.Default
            };

            if (dataPointOrTombstone.DataPoint != null)
                kafkaMessage.Value = InputDataPointSerializer.SerializeDataPoint(dataPointOrTombstone.DataPoint);

            return kafkaMessage;
        }

        private AttributeValue[] GetDataPointId(DataSourceDescriptor dataSource, InputDataPointOrTombstone dataPointOrTombstone)
        {
            var idAttributeKeys = idAttributeKeysByDataSourceId.GetOrAdd(
                dataSource.Id,
                _ =>
                {
                    var idAttributes = dataSource.Meta.IdAttributes.OrderBy(x => x, StringComparer.InvariantCulture).ToArray();
                    if (!idAttributes.Any())
                        throw new InvalidOperationException($"{nameof(idAttributes)} is empty for dataSource: {dataSource.ToPrettyJson()}");

                    return idAttributes;
                });

            var attributeValues = dataPointOrTombstone.GetAttributes();

            return idAttributeKeys.Select(key => attributeValues[key]).ToArray();
        }
    }
}
