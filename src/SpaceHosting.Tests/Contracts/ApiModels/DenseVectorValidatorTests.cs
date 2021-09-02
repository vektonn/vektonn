using System;
using System.Collections.Generic;
using NUnit.Framework;
using SpaceHosting.Contracts.ApiModels;
using SpaceHosting.Contracts.ApiModels.Validation;

namespace SpaceHosting.Tests.Contracts.ApiModels
{
    public class DenseVectorValidatorTests
    {
        [TestCaseSource(nameof(TestCases))]
        public string Validate(VectorDto vector, int vectorDimension)
        {
            var sut = new DenseVectorValidator(vectorDimension);

            return sut.Validate(vector).ToString();
        }

        private static IEnumerable<TestCaseData> TestCases()
        {
            yield return new TestCaseData(new SparseVectorDto(Coordinates: new double[1], CoordinateIndices: new int[1]), 1) {ExpectedResult = "Vector must be dense"};

            yield return new TestCaseData(new DenseVectorDto(Coordinates: Array.Empty<double>()), 1) {ExpectedResult = "Vector coordinates must have dimension: 1"};
            yield return new TestCaseData(new DenseVectorDto(Coordinates: new double[1]), 2) {ExpectedResult = "Vector coordinates must have dimension: 2"};
            yield return new TestCaseData(new DenseVectorDto(Coordinates: new double[3]), 2) {ExpectedResult = "Vector coordinates must have dimension: 2"};

            yield return new TestCaseData(new DenseVectorDto(Coordinates: new double[2]), 2) {ExpectedResult = string.Empty};
        }
    }
}
