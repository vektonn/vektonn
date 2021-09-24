namespace Vektonn.Contracts.ApiModels
{
    public record SearchResultDto(VectorDto QueryVector, FoundDataPointDto[] NearestDataPoints);
}
