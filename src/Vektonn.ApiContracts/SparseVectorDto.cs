namespace Vektonn.ApiContracts
{
    public record SparseVectorDto(double[] Coordinates, int[] CoordinateIndices)
        : VectorDto(IsSparse: true, Coordinates);
}
