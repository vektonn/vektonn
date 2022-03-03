namespace Vektonn.ApiContracts
{
    public record FoundDataPointDto(VectorDto? Vector, AttributeDto[] Attributes, double Distance);
}
