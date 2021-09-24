using System.Collections.Generic;
using Vektonn.Index;

namespace Vektonn.Contracts
{
    public record DataPoint<TVector>(TVector Vector, Dictionary<string, AttributeValue> Attributes)
        where TVector : IVector;
}
