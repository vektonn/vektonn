using System.Collections.Generic;

namespace Vektonn.SharedImpl.Contracts.Sharding.Index
{
    public record DataSourceShardSubscription(Dictionary<string, ulong?> ShardingCoordinatesByAttributeKey);
}
