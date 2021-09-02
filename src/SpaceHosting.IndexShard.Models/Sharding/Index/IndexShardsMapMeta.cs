using System.Collections.Generic;
using System.Linq;
using SpaceHosting.IndexShard.Models.ApiModels;

namespace SpaceHosting.IndexShard.Models.Sharding.Index
{
    public record IndexShardsMapMeta(Dictionary<string, IndexShardMeta> ShardsById)
    {
        public int TotalShardsCount => ShardsById.Count;
        public HashSet<string> ShardAttributes => ShardsById.SelectMany(shard => shard.Value.ShardsByAttributeKey.Keys).ToHashSet();

        public HashSet<string> GetShardIdsForQuery(AttributeDto[]? splitFilter)
        {
            return ShardsById
                .Where(shard => shard.Value.MatchesFilter(splitFilter))
                .Select(shard => shard.Key)
                .ToHashSet();
        }
    }
}
