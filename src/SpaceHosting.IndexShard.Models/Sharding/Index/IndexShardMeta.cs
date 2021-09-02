using System.Collections.Generic;
using SpaceHosting.IndexShard.Models.ApiModels;

namespace SpaceHosting.IndexShard.Models.Sharding.Index
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
