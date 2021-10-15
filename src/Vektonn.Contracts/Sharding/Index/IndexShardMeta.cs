using System;
using System.Collections.Generic;
using Vektonn.Contracts.ApiModels;

namespace Vektonn.Contracts.Sharding.Index
{
    public record IndexShardMeta(Dictionary<string, IIndexAttributeValueShard> ShardsByAttributeKey, DataSourceShardSubscription[] DataSourceShardsToConsume)
    {
        // todo (andrew, 09.09.2021): test
        // todo (andrew, 09.09.2021): maybe optimize for exact mapping between data source and index shards
        public bool Contains(Dictionary<string, AttributeValue> idAttributes)
        {
            foreach (var (attributeKey, indexAttributeValueShard) in ShardsByAttributeKey)
            {
                if (!idAttributes.TryGetValue(attributeKey, out var attributeValue))
                    throw new InvalidOperationException($"Index sharding attribute '{attributeKey}' is missing");

                if (!indexAttributeValueShard.Contains(attributeValue))
                    return false;
            }

            return true;
        }

        public bool MatchesFilter(AttributeDto[]? splitFilter)
        {
            if (splitFilter == null)
                return true;

            foreach (var (attributeKey, attributeValue) in splitFilter)
            {
                if (!ShardsByAttributeKey.TryGetValue(attributeKey, out var shard))
                    continue;

                if (!shard.Contains(attributeValue))
                    return false;
            }

            return true;
        }
    }
}
