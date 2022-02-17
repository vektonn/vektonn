using Vektonn.ApiContracts;
using Vektonn.DataSource;
using Vektonn.DataSource.Kafka;
using Vektonn.Index;
using Vektonn.IndexShard;
using Vektonn.SharedImpl.ApiContracts;
using Vektonn.SharedImpl.Contracts;
using Vostok.Logging.Abstractions;

namespace Vektonn.IndexShardService.Services
{
    public class IndexShardOwner<TVector> : IIndexShardOwner
        where TVector : IVector
    {
        private readonly IndexShardHolder<TVector> indexShardHolder;
        private readonly KafkaDataSourceConsumer<TVector> kafkaDataSourceConsumer;

        public IndexShardOwner(ILog log, IndexShardConfiguration indexShardConfiguration, TVector zeroVector)
        {
            indexShardHolder = new IndexShardHolder<TVector>(log, indexShardConfiguration.IndexMeta);

            kafkaDataSourceConsumer = new KafkaDataSourceConsumer<TVector>(
                log,
                indexShardConfiguration.KafkaConsumerConfig,
                indexShardConfiguration.IndexMeta.DataSourceMeta,
                indexShardConfiguration.IndexShardMeta,
                dataPointOrTombstones => indexShardHolder.UpdateIndexShard(dataPointOrTombstones));

            ZeroVector = zeroVector.ToVectorDto()!;
        }

        public IndexMeta IndexMeta => indexShardHolder.IndexMeta;
        public VectorDto ZeroVector { get; }
        public long DataPointsCount => indexShardHolder.DataPointsCount;

        public ISearchQueryExecutor SearchQueryExecutor => indexShardHolder;
        public IDataSourceConsumer DataSourceConsumer => kafkaDataSourceConsumer;

        public void Dispose()
        {
            kafkaDataSourceConsumer.Dispose();
            indexShardHolder.Dispose();
        }
    }
}
