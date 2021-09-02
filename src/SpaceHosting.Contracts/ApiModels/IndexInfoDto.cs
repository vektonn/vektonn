namespace SpaceHosting.Contracts.ApiModels
{
    public record IndexInfoDto(string IndexAlgorithm, string VectorType, int VectorDimension, long DataPointsCount);
}
