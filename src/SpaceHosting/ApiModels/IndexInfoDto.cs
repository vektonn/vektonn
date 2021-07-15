namespace SpaceHosting.ApiModels
{
    public class IndexInfoDto
    {
        public string IndexAlgorithm { get; init; } = null!;
        public string VectorType { get; init; } = null!;
        public int VectorDimension { get; init; }
        public int VectorCount { get; init; }
    }
}
