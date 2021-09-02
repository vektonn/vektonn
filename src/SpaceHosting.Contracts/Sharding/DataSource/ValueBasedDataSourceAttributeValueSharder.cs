using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SpaceHosting.Contracts.Sharding.DataSource
{
    public class ValueBasedDataSourceAttributeValueSharder : IDataSourceAttributeValueSharder
    {
        public ValueBasedDataSourceAttributeValueSharder(HashSet<AttributeValue> possibleValues)
        {
            PossibleValues = possibleValues;
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public HashSet<AttributeValue> PossibleValues { get; }

        public bool IsValueAcceptable(AttributeValue attributeValue)
        {
            return PossibleValues.Contains(attributeValue);
        }
    }
}
