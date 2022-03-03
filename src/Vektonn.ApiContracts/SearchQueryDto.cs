using System.Linq;

namespace Vektonn.ApiContracts
{
    public record SearchQueryDto(AttributeDto[]? SplitFilter, VectorDto[] QueryVectors, int K, bool RetrieveVectors)
    {
        public override string ToString()
        {
            return SplitFilter == null
                ? $"VectorsCount = {QueryVectors.Length}, K = {K}, RetrieveVectors = {RetrieveVectors}"
                : $"VectorsCount = {QueryVectors.Length}, K = {K}, RetrieveVectors = {RetrieveVectors}, SplitFilter = {string.Join(";", SplitFilter.Select(t => t.ToString()))}";
        }
    }
}
