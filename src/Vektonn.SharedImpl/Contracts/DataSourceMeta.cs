using System.Collections.Generic;
using System.Linq;
using Vektonn.SharedImpl.Contracts.Sharding.DataSource;

namespace Vektonn.SharedImpl.Contracts
{
    public record DataSourceMeta(
        DataSourceId Id,
        int VectorDimension,
        bool VectorsAreSparse,
        HashSet<string> PermanentAttributes,
        DataSourceShardingMeta DataSourceShardingMeta,
        Dictionary<string, AttributeValueTypeCode> AttributeValueTypes)
    {
        public HashSet<string> ShardAttributes => DataSourceShardingMeta.ShardersByAttributeKey.Keys.ToHashSet();

        public override string ToString()
        {
            return $"{nameof(Id)} = {Id}, " +
                   $"{nameof(VectorDimension)} = {VectorDimension}, " +
                   $"{nameof(VectorsAreSparse)} = {VectorsAreSparse}, " +
                   $"{nameof(PermanentAttributes)} = {string.Join(";", PermanentAttributes)}, " +
                   $"{nameof(ShardAttributes)} = {string.Join(";", ShardAttributes)}, " +
                   $"{nameof(AttributeValueTypes)} = {string.Join(";", AttributeValueTypes.Select(t => $"{t.Key}:{t.Value}"))}";
        }
    }
}
