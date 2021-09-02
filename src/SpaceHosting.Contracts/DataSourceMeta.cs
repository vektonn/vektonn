using System.Collections.Generic;
using System.Linq;
using SpaceHosting.Contracts.Sharding.DataSource;

namespace SpaceHosting.Contracts
{
    public record DataSourceMeta(
        int VectorDimension,
        bool VectorsAreSparse,
        HashSet<string> IdAttributes,
        DataSourceShardingMeta DataSourceShardingMeta,
        Dictionary<string, AttributeValueTypeCode> AttributeValueTypes)
    {
        public override string ToString()
        {
            return $"{nameof(VectorDimension)} = {VectorDimension}, " +
                   $"{nameof(VectorsAreSparse)} = {VectorsAreSparse}, " +
                   $"{nameof(IdAttributes)} = {string.Join(";", IdAttributes)}, " +
                   $"{nameof(DataSourceShardingMeta.ShardAttributes)} = {string.Join(";", DataSourceShardingMeta.ShardAttributes)}, " +
                   $"{nameof(AttributeValueTypes)} = {string.Join(";", AttributeValueTypes.Select(t => $"{t.Key}:{t.Value}"))}";
        }
    }
}
