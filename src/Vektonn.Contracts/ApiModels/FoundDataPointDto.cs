namespace Vektonn.Contracts.ApiModels
{
    public record FoundDataPointDto(VectorDto Vector, AttributeDto[] Attributes, double Distance);
}
