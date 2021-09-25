using System.Collections.Generic;

namespace Vektonn.Contracts.Sharding.Index
{
    public record DataSourceShardSubscription(Dictionary<string, ulong?> ShardingCoordinatesByAttributeKey);
}
