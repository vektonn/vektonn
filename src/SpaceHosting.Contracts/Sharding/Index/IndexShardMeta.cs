using System.Collections.Generic;
using SpaceHosting.Contracts.ApiModels;

namespace SpaceHosting.Contracts.Sharding.Index
{
    public record IndexShardMeta(Dictionary<string, IIndexAttributeValueShard> ShardsByAttributeKey)
    {
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
