namespace Vektonn.ApiContracts
{
    public record SearchResultDto(VectorDto QueryVector, FoundDataPointDto[] NearestDataPoints);
}
