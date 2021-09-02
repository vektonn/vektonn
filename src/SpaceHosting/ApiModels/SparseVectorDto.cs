using SpaceHosting.Index;

namespace SpaceHosting.ApiModels
{
    public class SparseVectorDto : VectorDto
    {
        public int[] CoordinateIndices { get; init; } = null!;

        public static explicit operator SparseVector(SparseVectorDto dto) => new SparseVector(dto.Dimension, dto.Coordinates, dto.CoordinateIndices);

        public static explicit operator SparseVectorDto(SparseVector vector) => new SparseVectorDto
        {
            IsSparse = true,
            Dimension = vector.Dimension,
            Coordinates = vector.Coordinates,
            CoordinateIndices = vector.CoordinateIndices
        };
    }
}
