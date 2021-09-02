namespace SpaceHosting.IndexShard.Models.ApiModels
{
    public record InputDataPointDto(AttributeDto[] Attributes, VectorDto? Vector, bool IsDeleted);
}
