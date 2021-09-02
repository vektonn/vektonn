using System.Linq;

namespace SpaceHosting.IndexShard.Models.ApiModels
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
