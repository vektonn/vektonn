using SpaceHosting.Index;

namespace SpaceHosting.ApiModels
{
    public class DenseVectorDto : VectorDto
    {
        public static explicit operator DenseVector(DenseVectorDto dto) => new DenseVector(dto.Coordinates);

        public static explicit operator DenseVectorDto(DenseVector vector) => new DenseVectorDto
        {
            IsSparse = false,
            Dimension = vector.Coordinates.Length,
            Coordinates = vector.Coordinates
        };
    }
}
