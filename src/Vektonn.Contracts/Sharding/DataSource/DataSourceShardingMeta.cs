using System.Collections.Generic;
using System.Linq;

namespace Vektonn.Contracts.Sharding.DataSource
{
    public record DataSourceShardingMeta(Dictionary<string, IDataSourceAttributeValueSharder> ShardersByAttributeKey)
    {
        public HashSet<string> ShardAttributes => ShardersByAttributeKey.Keys.ToHashSet();
    }
}
