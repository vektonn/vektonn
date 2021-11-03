namespace Vektonn.ApiContracts
{
    public record IndexInfoDto(string IndexAlgorithm, string VectorType, int VectorDimension, long DataPointsCount);
}
