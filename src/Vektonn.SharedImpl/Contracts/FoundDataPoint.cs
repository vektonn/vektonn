using System.Collections.Generic;
using Vektonn.Index;

namespace Vektonn.SharedImpl.Contracts
{
    public record FoundDataPoint<TVector>(TVector? Vector, Dictionary<string, AttributeValue> Attributes, double Distance)
        where TVector : IVector;
}
