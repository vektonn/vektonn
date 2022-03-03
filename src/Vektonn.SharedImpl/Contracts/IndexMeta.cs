using System;
using System.Collections.Generic;
using System.Linq;
using Vektonn.Index;
using Vektonn.SharedImpl.Contracts.Sharding.Index;

namespace Vektonn.SharedImpl.Contracts
{
    public record IndexMeta(
        IndexId Id,
        DataSourceMeta DataSourceMeta,
        IndexAlgorithm IndexAlgorithm,
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
            DataSourceMeta.ValidateConsistency();

            var untypedAttributes = IdAttributes.Union(SplitAttributes).Union(ShardAttributes).Except(DataSourceMeta.AttributeValueTypes.Keys).ToArray();
            if (untypedAttributes.Any())
                throw new InvalidOperationException($"There are attributes with unspecified value type ({string.Join(", ", untypedAttributes)}) for index: {this}");

            if (!IndexIdAttributes.Any())
                throw new InvalidOperationException($"{nameof(IndexIdAttributes)} is empty for index: {this}");

            var invalidIdAttributes = IdAttributes.Except(DataSourceMeta.PermanentAttributes).ToArray();
            if (invalidIdAttributes.Any())
                throw new InvalidOperationException($"There are id attributes ({string.Join(", ", invalidIdAttributes)}) which do not belong to permanent attributes for index: {this}");

            var invalidShardingAttributes = ShardAttributes.Except(DataSourceMeta.PermanentAttributes).ToArray();
            if (invalidShardingAttributes.Any())
                throw new InvalidOperationException($"There are sharding attributes ({string.Join(", ", invalidShardingAttributes)}) which do not belong to permanent attributes for index: {this}");

            var invalidSplitAttributes = SplitAttributes.Except(DataSourceMeta.PermanentAttributes).ToArray();
            if (invalidSplitAttributes.Any())
                throw new InvalidOperationException($"There are split attributes ({string.Join(", ", invalidSplitAttributes)}) which do not belong to permanent attributes for index: {this}");

            var vectorsAreSparse = AlgorithmTraits.VectorsAreSparse(IndexAlgorithm.Type);
            if (vectorsAreSparse ^ DataSourceMeta.VectorsAreSparse)
                throw new InvalidOperationException($"IndexMeta.VectorsAreSparse ({vectorsAreSparse}) and DataSourceMeta.VectorsAreSparse ({DataSourceMeta.VectorsAreSparse}) are inconsistent");
        }

        public override string ToString()
        {
            return $"{nameof(Id)} = {Id}, " +
                   $"{nameof(IndexAlgorithm)} = {IndexAlgorithm}, " +
                   $"{nameof(IdAttributes)} = {string.Join(";", IdAttributes)}, " +
                   $"{nameof(SplitAttributes)} = {string.Join(";", SplitAttributes)}, " +
                   $"{nameof(ShardAttributes)} = {string.Join(";", ShardAttributes)}, " +
                   $"{nameof(IndexPayloadAttributes)} = {string.Join(";", IndexPayloadAttributes)}, " +
                   $"{nameof(DataSourceMeta)} = {DataSourceMeta}";
        }
    }
}
