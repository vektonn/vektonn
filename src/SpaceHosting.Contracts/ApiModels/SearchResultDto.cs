namespace SpaceHosting.Contracts.ApiModels
{
    public record SearchResultDto(VectorDto QueryVector, FoundDataPointDto[] NearestKDataPoints);
}
