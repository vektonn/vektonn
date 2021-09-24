namespace Vektonn.Contracts.ApiModels
{
    public record DenseVectorDto(double[] Coordinates)
        : VectorDto(IsSparse: false, Coordinates);
}
