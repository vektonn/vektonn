using SpaceHosting.Index;

namespace SpaceHosting.Service.Models
{
    public class SearchResultDto
    {
        public double Distance { get; init; }
        public object Vector { get; init; } = null!;
        public object? Data { get; init; }
    }
}
