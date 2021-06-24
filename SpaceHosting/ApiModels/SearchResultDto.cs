using SpaceHosting.Index;

namespace SpaceHosting.ApiModels
{
    public class SearchResultDto
    {
        public double Distance { get; init; }
        public IVector Vector { get; init; } = null!;
        public object? Data { get; init; }
    }
}
