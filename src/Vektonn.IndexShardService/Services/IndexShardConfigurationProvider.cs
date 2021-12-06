using System;
using Vektonn.DataSource.Kafka;
using Vektonn.Hosting;
using Vektonn.Hosting.Configuration;
using Vektonn.SharedImpl.Configuration;
using Vektonn.SharedImpl.Contracts;
using Vektonn.SharedImpl.Contracts.Sharding.Index;

namespace Vektonn.IndexShardService.Services
{
    public class IndexShardConfigurationProvider
    {
        private readonly IIndexMetaProvider indexMetaProvider;
        private readonly KafkaConfigurationProvider kafkaConfigurationProvider;

        public IndexShardConfigurationProvider(IIndexMetaProvider indexMetaProvider, KafkaConfigurationProvider kafkaConfigurationProvider)
        {
            this.indexMetaProvider = indexMetaProvider;
            this.kafkaConfigurationProvider = kafkaConfigurationProvider;
        }

        public IndexShardConfiguration GetConfiguration()
        {
            var indexMeta = GetIndexMeta();
            var indexShardMeta = GetIndexShardMeta(indexMeta);
            var kafkaBootstrapServers = kafkaConfigurationProvider.GetKafkaBootstrapServers();

            return new IndexShardConfiguration(
                indexMeta,
                indexShardMeta,
                new KafkaConsumerConfig(kafkaBootstrapServers));
        }

        private static IndexShardMeta GetIndexShardMeta(IndexMeta indexMeta)
        {
            var indexShardId = EnvironmentVariables.Get("VEKTONN_INDEX_SHARD_ID");

            if (!indexMeta.IndexShardsMap.ShardsById.TryGetValue(indexShardId, out var indexShardMeta))
                throw new InvalidOperationException($"Failed to get indexShardMeta for indexShardId: '{indexShardId}', indexMeta: {indexMeta}");

            return indexShardMeta;
        }

        private IndexMeta GetIndexMeta()
        {
            var indexName = EnvironmentVariables.Get("VEKTONN_INDEX_NAME");
            var indexVersion = EnvironmentVariables.Get("VEKTONN_INDEX_VERSION");
            var indexId = new IndexId(indexName, indexVersion);

            var indexMetaWithShardEndpoints = indexMetaProvider.TryGetIndexMeta(indexId);
            if (indexMetaWithShardEndpoints == null)
                throw new InvalidOperationException($"Failed to get indexMeta for: {indexId}");

            return indexMetaWithShardEndpoints.IndexMeta;
        }
    }
}
