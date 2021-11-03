using Vektonn.ApiContracts;

namespace Vektonn.Tests.SharedImpl.ApiContracts
{
    public static class VectorDtoTestHelpers
    {
        public const int TestVectorDimension = 3;

        public static VectorDto[] SparseQueryVectors()
        {
            return new VectorDto[] {SparseVector(), SparseVector()};
        }

        public static SparseVectorDto SparseVector()
        {
            return new SparseVectorDto(new double[1], new[] {TestVectorDimension - 1});
        }

        public static VectorDto[] DenseQueryVectors()
        {
            return new VectorDto[] {DenseVector(), DenseVector()};
        }

        public static DenseVectorDto DenseVector()
        {
            return new DenseVectorDto(new double[TestVectorDimension]);
        }
    }
}
