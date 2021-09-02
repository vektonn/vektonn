using System.Collections.Generic;
using SpaceHosting.Index;

namespace SpaceHosting.Contracts
{
    public record FoundDataPoint<TVector>(TVector Vector, Dictionary<string, AttributeValue> Attributes, double Distance)
        : DataPoint<TVector>(Vector, Attributes)
        where TVector : IVector;
}
