using System.Linq;

namespace SpaceHosting.Contracts.ApiModels
{
    public record SearchQueryDto(AttributeDto[]? SplitFilter, VectorDto[] QueryVectors, int K)
    {
        public override string ToString()
        {
            return SplitFilter == null
                ? $"VectorsCount = {QueryVectors.Length}, K = {K}"
                : $"VectorsCount = {QueryVectors.Length}, K = {K}, SplitFilter = {string.Join(";", SplitFilter.Select(t => t.ToString()))}";
        }
    }
}
