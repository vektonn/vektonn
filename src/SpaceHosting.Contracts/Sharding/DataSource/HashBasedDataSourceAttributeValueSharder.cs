using System.Diagnostics.CodeAnalysis;

namespace SpaceHosting.Contracts.Sharding.DataSource
{
    public class HashBasedDataSourceAttributeValueSharder : IDataSourceAttributeValueSharder
    {
        private readonly ShardAttributeValueProjector shardAttributeValueProjector;

        public HashBasedDataSourceAttributeValueSharder(ushort numberOfShards, IAttributeValueHasher attributeValueHasher)
        {
            NumberOfShards = numberOfShards;
            shardAttributeValueProjector = new ShardAttributeValueProjector(numberOfShards, attributeValueHasher);
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public ushort NumberOfShards { get; }

        public bool IsValueAcceptable(AttributeValue attributeValue)
        {
            return true;
        }
    }
}
