namespace SpaceHosting.ApiModels
{
    public class SearchQueryDto
    {
        public int K { get; init; }
        public VectorDto[] Vectors { get; init; } = null!;
    }
}
