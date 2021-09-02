namespace SpaceHosting.IndexShard.Models.ApiModels
{
    public record SearchResultDto(VectorDto QueryVector, FoundDataPointDto[] NearestKDataPoints);
}
