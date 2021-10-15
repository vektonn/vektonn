using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Vektonn.Contracts.Sharding.DataSource
{
    public class ValueBasedDataSourceAttributeValueSharder : IDataSourceAttributeValueSharder
    {
        private readonly IAttributeValueHasher attributeValueHasher;

        public ValueBasedDataSourceAttributeValueSharder(IAttributeValueHasher attributeValueHasher, HashSet<AttributeValue> possibleValues)
        {
            this.attributeValueHasher = attributeValueHasher;
            PossibleValues = possibleValues;
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public HashSet<AttributeValue> PossibleValues { get; }

        public bool IsValueAcceptable(AttributeValue attributeValue)
        {
            return PossibleValues.Contains(attributeValue);
        }

        public ulong GetShardingCoordinate(AttributeValue attributeValue)
        {
            return attributeValueHasher.ComputeHash(attributeValue);
        }
    }
}
