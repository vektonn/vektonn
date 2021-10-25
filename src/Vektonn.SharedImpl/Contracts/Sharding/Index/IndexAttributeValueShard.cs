using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Vektonn.SharedImpl.Contracts.Sharding.Index
{
    public class IndexAttributeValueShard<TProjection> : IIndexAttributeValueShard
    {
        private readonly IAttributeValueProjector<TProjection> attributeValueProjector;

        public IndexAttributeValueShard(
            IndexShardingRule shardingRule,
            HashSet<TProjection> shardValues,
            IAttributeValueProjector<TProjection> attributeValueProjector)
        {
            ShardingRule = shardingRule;
            ShardValues = shardValues;
            this.attributeValueProjector = attributeValueProjector;
        }

        public IndexShardingRule ShardingRule { get; }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public HashSet<TProjection> ShardValues { get; }

        public bool Contains(AttributeValue attributeValue)
        {
            var projection = attributeValueProjector.GetProjection(attributeValue);

            return ShardingRule switch
            {
                IndexShardingRule.BelongToSet => ShardValues.Contains(projection),
                IndexShardingRule.BelongToComplementSet => !ShardValues.Contains(projection),
                _ => throw new InvalidOperationException($"Invalid {nameof(ShardingRule)}: {ShardingRule}")
            };
        }
    }
}
