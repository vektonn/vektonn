using System;
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

        public string[] GetPermanentAttributeKeysOrdered()
        {
            return PermanentAttributes.OrderBy(x => x, StringComparer.InvariantCulture).ToArray();
        }

        public void ValidateConsistency()
        {
            if (!PermanentAttributes.Any())
                throw new InvalidOperationException($"{nameof(PermanentAttributes)} is empty for dataSource: {this}");

            var untypedAttributes = PermanentAttributes.Union(ShardAttributes).Except(AttributeValueTypes.Keys).ToArray();
            if (untypedAttributes.Any())
                throw new InvalidOperationException($"There are attributes with unspecified value type ({string.Join(", ", untypedAttributes)}) for dataSource: {this}");

            var invalidShardingAttributes = ShardAttributes.Except(PermanentAttributes).ToArray();
            if (invalidShardingAttributes.Any())
                throw new InvalidOperationException($"There are sharding attributes ({string.Join(", ", invalidShardingAttributes)}) which do not belong to permanent attributes for dataSource: {this}");
        }

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
