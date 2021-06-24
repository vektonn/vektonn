using SpaceHosting.Index;

namespace SpaceHosting.ApiModels
{
    public class SearchQueryDto
    {
        public int K { get; init; }
        public IVector[] Vectors { get; init; } = null!;
    }
}
