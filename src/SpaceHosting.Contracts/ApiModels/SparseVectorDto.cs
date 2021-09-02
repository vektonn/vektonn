namespace SpaceHosting.Contracts.ApiModels
{
    public record SparseVectorDto(double[] Coordinates, int[] CoordinateIndices)
        : VectorDto(IsSparse: true, Coordinates);
}
