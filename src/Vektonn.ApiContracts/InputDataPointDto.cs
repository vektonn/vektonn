namespace Vektonn.ApiContracts
{
    public record InputDataPointDto(AttributeDto[] Attributes, VectorDto? Vector, bool IsDeleted);
}
