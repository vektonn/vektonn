namespace SpaceHosting.IndexShard.Models.ApiModels
{
    public record SparseVectorDto(double[] Coordinates, int[] CoordinateIndices)
        : VectorDto(IsSparse: true, Coordinates);
}
