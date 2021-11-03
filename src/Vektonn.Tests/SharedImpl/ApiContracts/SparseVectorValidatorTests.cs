using System;
using System.Collections.Generic;
using NUnit.Framework;
using Vektonn.ApiContracts;
using Vektonn.SharedImpl.ApiContracts.Validation;

namespace Vektonn.Tests.SharedImpl.ApiContracts
{
    public class SparseVectorValidatorTests
    {
        [TestCaseSource(nameof(TestCases))]
        public string Validate(VectorDto vector, int vectorDimension)
        {
            var sut = new SparseVectorValidator(vectorDimension);

            return sut.Validate(vector).ToString();
        }

        private static IEnumerable<TestCaseData> TestCases()
        {
            yield return new TestCaseData(new DenseVectorDto(Coordinates: new double[1]), 1) {ExpectedResult = "Vector must be sparse"};

            yield return new TestCaseData(new SparseVectorDto(Coordinates: Array.Empty<double>(), CoordinateIndices: Array.Empty<int>()), 1) {ExpectedResult = "Vector must have coordinates"};

            yield return new TestCaseData(new SparseVectorDto(Coordinates: new double[3], CoordinateIndices: new[] {0, 23}), 24) {ExpectedResult = "Each vector coordinate value must have unique corresponding index"};
            yield return new TestCaseData(new SparseVectorDto(Coordinates: new double[3], CoordinateIndices: new[] {0, 7, 11, 23}), 24) {ExpectedResult = "Each vector coordinate value must have unique corresponding index"};
            yield return new TestCaseData(new SparseVectorDto(Coordinates: new double[3], CoordinateIndices: new[] {7, 7, 23}), 24) {ExpectedResult = "Each vector coordinate value must have unique corresponding index"};

            yield return new TestCaseData(new SparseVectorDto(Coordinates: new double[3], CoordinateIndices: new[] {-1, 7, 23}), 24) {ExpectedResult = "Vector coordinates must have dimension: 24"};
            yield return new TestCaseData(new SparseVectorDto(Coordinates: new double[3], CoordinateIndices: new[] {0, 7, 24}), 24) {ExpectedResult = "Vector coordinates must have dimension: 24"};

            yield return new TestCaseData(new SparseVectorDto(Coordinates: new double[3], CoordinateIndices: new[] {0, 7, 23}), 24) {ExpectedResult = string.Empty};
            yield return new TestCaseData(new SparseVectorDto(Coordinates: new double[3], CoordinateIndices: new[] {7, 23, 0}), 24) {ExpectedResult = string.Empty};
        }
    }
}
