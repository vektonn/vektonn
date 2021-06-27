using System;
using System.Collections.Generic;
using System.Linq;
using SpaceHosting.Index;

namespace SpaceHosting.ApiModels
{
    public static class VectorConversions
    {
        public static IVector ToVector(this VectorDto dto) => dto switch
        {
            DenseVectorDto denseVectorDto => (DenseVector)denseVectorDto,
            SparseVectorDto sparseVectorDto => (SparseVector)sparseVectorDto,
            _ => throw new ArgumentException($"Invalid VectorDto type: {dto.GetType()}")
        };

        public static VectorDto ToVectorDto(this IVector vector) => vector switch
        {
            DenseVector denseVector => (DenseVectorDto)denseVector,
            SparseVector sparseVector => (SparseVectorDto)sparseVector,
            _ => throw new ArgumentException($"Invalid Vector type: {vector.GetType()}")
        };

        public static VectorDto[] ToVectorDto(this IEnumerable<IVector> vectors) => vectors.Select(v => v.ToVectorDto()).ToArray();
    }
}
