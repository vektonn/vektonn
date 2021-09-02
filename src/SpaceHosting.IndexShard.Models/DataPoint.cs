using System.Collections.Generic;
using SpaceHosting.Index;

namespace SpaceHosting.IndexShard.Models
{
    public record DataPoint<TVector>(TVector Vector, Dictionary<string, AttributeValue> Attributes)
        where TVector : IVector;
}
