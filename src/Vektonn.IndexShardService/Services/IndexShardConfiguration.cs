using Vektonn.DataSource.Kafka;
using Vektonn.SharedImpl.Contracts;
using Vektonn.SharedImpl.Contracts.Sharding.Index;

namespace Vektonn.IndexShardService.Services
{
    public record IndexShardConfiguration(
        IndexMeta IndexMeta,
        IndexShardMeta IndexShardMeta,
        KafkaConsumerConfig KafkaConsumerConfig);
}
