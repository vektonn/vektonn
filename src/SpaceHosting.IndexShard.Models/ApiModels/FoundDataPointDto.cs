namespace SpaceHosting.IndexShard.Models.ApiModels
{
    public record FoundDataPointDto(VectorDto Vector, AttributeDto[] Attributes, double Distance);
}
