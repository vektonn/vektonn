using System;
using System.Collections.Generic;

namespace Vektonn.Contracts.Sharding.DataSource
{
    public record DataSourceShardingMeta(Dictionary<string, IDataSourceAttributeValueSharder> ShardersByAttributeKey)
    {
        // todo (andrew, 09.09.2021): test
        public Dictionary<string, ulong> GetDataSourceShardingCoordinates(Dictionary<string, AttributeValue> attributes)
        {
            var shardingCoordinatesByAttributeKey = new Dictionary<string, ulong>();
            foreach (var (attributeKey, attributeValueSharder) in ShardersByAttributeKey)
            {
                if (!attributes.TryGetValue(attributeKey, out var attributeValue))
                    throw new InvalidOperationException($"DataSource sharding attribute '{attributeKey}' is missing");

                var shardingCoordinate = attributeValueSharder.GetShardingCoordinate(attributeValue);
                shardingCoordinatesByAttributeKey.Add(attributeKey, shardingCoordinate);
            }

            return shardingCoordinatesByAttributeKey;
        }
    }
}
