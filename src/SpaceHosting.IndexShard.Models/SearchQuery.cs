using System.Collections.Generic;
using System.Linq;
using SpaceHosting.Index;

namespace SpaceHosting.IndexShard.Models
{
    public record SearchQuery<TVector>(Dictionary<string, AttributeValue>? SplitFilter, TVector[] QueryVectors, int K)
        where TVector : IVector
    {
        public override string ToString()
        {
            return SplitFilter == null
                ? $"VectorsCount = {QueryVectors.Length}, K = {K}"
                : $"VectorsCount = {QueryVectors.Length}, K = {K}, SplitFilter = {string.Join(";", SplitFilter.Select(t => $"{t.Key}:{t.Value}"))}";
        }
    }
}
