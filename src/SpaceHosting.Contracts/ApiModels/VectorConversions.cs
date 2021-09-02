using System;
using SpaceHosting.Index;

namespace SpaceHosting.Contracts.ApiModels
{
    public static class VectorConversions
    {
        public static IVector ToVector(this VectorDto dto, int vectorDimension) => dto switch
        {
            DenseVectorDto denseVectorDto => new DenseVector(denseVectorDto.Coordinates),
            SparseVectorDto sparseVectorDto => new SparseVector(vectorDimension, sparseVectorDto.Coordinates, sparseVectorDto.CoordinateIndices),
            _ => throw new ArgumentException($"Invalid VectorDto type: {dto.GetType()}")
        };

        public static VectorDto ToVectorDto(this IVector vector) => vector switch
        {
            DenseVector denseVector => new DenseVectorDto(denseVector.Coordinates),
            SparseVector sparseVector => new SparseVectorDto(sparseVector.Coordinates, sparseVector.CoordinateIndices),
            _ => throw new ArgumentException($"Invalid Vector type: {vector.GetType()}")
        };
    }
}
