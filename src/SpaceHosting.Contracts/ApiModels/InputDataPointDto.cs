namespace SpaceHosting.Contracts.ApiModels
{
    public record InputDataPointDto(AttributeDto[] Attributes, VectorDto? Vector, bool IsDeleted);
}
