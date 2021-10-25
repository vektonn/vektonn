using System.Collections.Generic;
using System.Linq;

namespace Vektonn.SharedImpl.Contracts.Sharding.Index
{
    public record IndexShardsMapMeta(Dictionary<string, IndexShardMeta> ShardsById)
    {
        public int TotalShardsCount => ShardsById.Count;
        public HashSet<string> ShardAttributes => ShardsById.SelectMany(shard => shard.Value.ShardsByAttributeKey.Keys).ToHashSet();

        public HashSet<string> GetShardIdsForQuery((string AttributeKey, AttributeValue)[]? splitFilter)
        {
            return ShardsById
                .Where(shard => shard.Value.MatchesFilter(splitFilter))
                .Select(shard => shard.Key)
                .ToHashSet();
        }
    }
}
