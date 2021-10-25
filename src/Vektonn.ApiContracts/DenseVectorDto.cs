namespace Vektonn.ApiContracts
{
    public record DenseVectorDto(double[] Coordinates)
        : VectorDto(IsSparse: false, Coordinates);
}
