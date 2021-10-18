using System;
using System.Collections.Generic;
using System.Linq;
using Vektonn.Contracts.Sharding.Index;
using Vektonn.Index;

namespace Vektonn.Contracts
{
    public record IndexMeta(
        DataSourceMeta DataSourceMeta,
        string IndexAlgorithm,
        HashSet<string> IdAttributes,
        HashSet<string> SplitAttributes,
        IndexShardsMapMeta IndexShardsMap)
    {
        public int VectorDimension => DataSourceMeta.VectorDimension;

        public HashSet<string> ShardAttributes => IndexShardsMap.ShardAttributes.ToHashSet();

        public HashSet<string> IndexIdAttributes => IdAttributes.Except(SplitAttributes).ToHashSet();
        public HashSet<string> IndexPayloadAttributes => DataSourceMeta.AttributeValueTypes.Keys.Except(IdAttributes).Except(SplitAttributes).ToHashSet();

        public bool HasSplits => SplitAttributes.Any();
        public bool HasPayload => IndexPayloadAttributes.Any();

        public void ValidateConsistency()
        {
            var untypedAttributes = DataSourceMeta.PermanentAttributes
                .Union(DataSourceMeta.ShardAttributes)
                .Union(IdAttributes)
                .Union(SplitAttributes)
                .Union(ShardAttributes)
                .Except(DataSourceMeta.AttributeValueTypes.Keys)
                .ToArray();
            if (untypedAttributes.Any())
                throw new InvalidOperationException($"There are attributes with unspecified value type ({string.Join(", ", untypedAttributes)}) for: {this}");

            var invalidDataSourceShardingAttributes = DataSourceMeta.ShardAttributes.Except(DataSourceMeta.PermanentAttributes).ToArray();
            if (invalidDataSourceShardingAttributes.Any())
                throw new InvalidOperationException($"There are data source sharding attributes ({string.Join(", ", invalidDataSourceShardingAttributes)}) which do not belong to permanent attributes for: {this}");

            var invalidIndexShardingAttributes = ShardAttributes.Except(DataSourceMeta.PermanentAttributes).ToArray();
            if (invalidIndexShardingAttributes.Any())
                throw new InvalidOperationException($"There are index sharding attributes ({string.Join(", ", invalidIndexShardingAttributes)}) which do not belong to permanent attributes for: {this}");

            var invalidSplitAttributes = SplitAttributes.Except(DataSourceMeta.PermanentAttributes).ToArray();
            if (invalidSplitAttributes.Any())
                throw new InvalidOperationException($"There are split attributes ({string.Join(", ", invalidSplitAttributes)}) which do not belong to permanent attributes for: {this}");

            var vectorsAreSparse = AlgorithmTraits.VectorsAreSparse(IndexAlgorithm);
            if (vectorsAreSparse ^ DataSourceMeta.VectorsAreSparse)
                throw new InvalidOperationException($"IndexMeta.VectorsAreSparse ({vectorsAreSparse}) and DataSourceMeta.VectorsAreSparse ({DataSourceMeta.VectorsAreSparse}) are inconsistent");
        }

        public override string ToString()
        {
            return $"{nameof(IndexAlgorithm)} = {IndexAlgorithm}, " +
                   $"{nameof(IdAttributes)} = {string.Join(";", IdAttributes)}, " +
                   $"{nameof(SplitAttributes)} = {string.Join(";", SplitAttributes)}, " +
                   $"{nameof(ShardAttributes)} = {string.Join(";", ShardAttributes)}, " +
                   $"{nameof(IndexPayloadAttributes)} = {string.Join(";", IndexPayloadAttributes)}, " +
                   $"{nameof(DataSourceMeta)} = {DataSourceMeta}";
        }
    }
}
