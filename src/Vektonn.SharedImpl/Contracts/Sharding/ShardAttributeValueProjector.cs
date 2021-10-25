using System;

namespace Vektonn.SharedImpl.Contracts.Sharding
{
    public class ShardAttributeValueProjector : IAttributeValueProjector<ushort>
    {
        private readonly ushort numberOfShards;
        private readonly IAttributeValueHasher attributeValueHasher;

        public ShardAttributeValueProjector(ushort numberOfShards, IAttributeValueHasher attributeValueHasher)
        {
            if (numberOfShards == 0)
                throw new InvalidOperationException($"{nameof(numberOfShards)} == 0");

            this.numberOfShards = numberOfShards;
            this.attributeValueHasher = attributeValueHasher;
        }

        public ushort GetProjection(AttributeValue attributeValue)
        {
            var hash = attributeValueHasher.ComputeHash(attributeValue);
            return (ushort)(hash % numberOfShards);
        }
    }
}
