using System.Collections.Generic;
using System.Linq;
using Vektonn.Index;

namespace Vektonn.SharedImpl.Contracts
{
    public record SearchQuery<TVector>(Dictionary<string, AttributeValue>? SplitFilter, TVector[] QueryVectors, int K, bool RetrieveVectors)
        where TVector : IVector
    {
        public override string ToString()
        {
            return SplitFilter == null
                ? $"VectorsCount = {QueryVectors.Length}, K = {K}, RetrieveVectors = {RetrieveVectors}"
                : $"VectorsCount = {QueryVectors.Length}, K = {K}, RetrieveVectors = {RetrieveVectors}, SplitFilter = {string.Join(";", SplitFilter.Select(t => $"{t.Key}:{t.Value}"))}";
        }
    }
}
