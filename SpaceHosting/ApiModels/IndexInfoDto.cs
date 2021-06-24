namespace SpaceHosting.ApiModels
{
    public class IndexInfoDto
    {
        public int VectorDimension { get; init; }
        public int VectorCount { get; init; }
        public string Description { get; init; } = null!;
    }
}
