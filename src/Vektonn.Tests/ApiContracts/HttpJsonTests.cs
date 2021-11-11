using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Vektonn.ApiContracts;
using Vektonn.ApiContracts.Json;
using Vektonn.SharedImpl.ApiContracts;
using Vektonn.SharedImpl.Json;
using static Vektonn.Tests.SharedImpl.ApiContracts.AttributeDtoTestHelpers;

namespace Vektonn.Tests.ApiContracts
{
    public class HttpJsonTests
    {
        [Test]
        public void Serialize_VectorDto_Dense()
        {
            var dto = new DenseVectorDto(Coordinates: new[] {1.0, 0.0, 0.1});
            SerializeToHttpJson(dto)
                .Should()
                .Be("{\"isSparse\":false,\"coordinates\":[1,0,0.1]}");
        }

        [Test]
        public void Serialize_VectorDto_Sparse()
        {
            var dto = new SparseVectorDto(Coordinates: new[] {1.0, 0.0, 0.1}, CoordinateIndices: new[] {7, 0, 42});
            SerializeToHttpJson(dto)
                .Should()
                .Be("{\"isSparse\":true,\"coordinates\":[1,0,0.1],\"coordinateIndices\":[7,0,42]}");
        }

        [TestCaseSource(nameof(Serialize_AttributeDto_TestCases))]
        public string Serialize_AttributeDto(AttributeDto attributeDto)
        {
            return SerializeToHttpJson(attributeDto);
        }

        [Test]
        public void Serialize_ErrorDto()
        {
            var dto = new ErrorDto(ErrorMessages: new[] {"First error message", "Second error message"});
            SerializeToHttpJson(dto)
                .Should()
                .Be("{\"errorMessages\":[\"First error message\",\"Second error message\"]}");
        }

        [Test]
        public void Serialize_SearchResultDto()
        {
            var dto = new SearchResultDto(
                QueryVector: new DenseVectorDto(Coordinates: new[] {1.0, 0.0, 0.1}),
                new[]
                {
                    new FoundDataPointDto(
                        Vector: new DenseVectorDto(Coordinates: new[] {1.0, 0.0, 0.1}),
                        Attributes: new[] {Attribute("Id", value: 23)},
                        Distance: 0),
                    new FoundDataPointDto(
                        Vector: new SparseVectorDto(Coordinates: new[] {-1.0, 0.0, -0.1}, CoordinateIndices: new[] {7, 0, 42}),
                        Attributes: new[]
                        {
                            Attribute("Id", value: -1),
                            Attribute("Data", "some payload"),
                        },
                        Distance: 0.2),
                });
            SerializeToHttpJson(dto)
                .Should()
                .Be("{\"queryVector\":{\"isSparse\":false,\"coordinates\":[1,0,0.1]},\"nearestDataPoints\":[{\"vector\":{\"isSparse\":false,\"coordinates\":[1,0,0.1]},\"attributes\":[{\"key\":\"Id\",\"value\":{\"int64\":23}}],\"distance\":0},{\"vector\":{\"isSparse\":true,\"coordinates\":[-1,0,-0.1],\"coordinateIndices\":[7,0,42]},\"attributes\":[{\"key\":\"Id\",\"value\":{\"int64\":-1}},{\"key\":\"Data\",\"value\":{\"string\":\"some payload\"}}],\"distance\":0.2}]}");
        }

        [Test]
        public void SerializeAndDeserialize_DtoWithDenseVector()
        {
            var denseVector = RandomHelpers.NextDenseVector(dimension: 7).ToVectorDto();
            SerializeAndDeserializeDtoWithVector(new Dto {Vector = denseVector});
            SerializeAndDeserializeDtoWithVector(new RecordDto(denseVector));
        }

        [Test]
        public void SerializeAndDeserialize_DtoWithSparseVector()
        {
            var sparseVector = RandomHelpers.NextSparseVector(dimension: 100, coordinatesCount: 5).ToVectorDto();
            SerializeAndDeserializeDtoWithVector(new Dto {Vector = sparseVector});
            SerializeAndDeserializeDtoWithVector(new RecordDto(sparseVector));
        }

        private static IEnumerable<TestCaseData> Serialize_AttributeDto_TestCases()
        {
            yield return new TestCaseData(
                Attribute("Str_Key", "Str_Value")
            ) {ExpectedResult = "{\"key\":\"Str_Key\",\"value\":{\"string\":\"Str_Value\"}}"};

            yield return new TestCaseData(
                Attribute("GuidKey", Guid.Parse("12345678-1234-1234-1234-123456789abc"))
            ) {ExpectedResult = "{\"key\":\"GuidKey\",\"value\":{\"guid\":\"12345678-1234-1234-1234-123456789abc\"}}"};

            yield return new TestCaseData(
                Attribute("boolKey", true)
            ) {ExpectedResult = "{\"key\":\"boolKey\",\"value\":{\"bool\":true}}"};

            yield return new TestCaseData(
                Attribute("int_key", 42)
            ) {ExpectedResult = "{\"key\":\"int_key\",\"value\":{\"int64\":42}}"};

            yield return new TestCaseData(
                Attribute("date", new DateTime(2021, 11, 23, 23, 59, 01))
            ) {ExpectedResult = "{\"key\":\"date\",\"value\":{\"dateTime\":\"2021-11-23T23:59:01\"}}"};
        }

        private static void SerializeAndDeserializeDtoWithVector<TDto>(TDto dto)
        {
            Console.Out.WriteLine(dto.ToPrettyJson());

            var httpJson = SerializeToHttpJson(dto);
            Console.Out.WriteLine(httpJson);

            var deserialized = JsonSerializer.Deserialize<TDto>(httpJson, HttpJson.Options);

            deserialized.Should().BeEquivalentTo(dto);
        }

        private static string SerializeToHttpJson<TDto>(TDto dto)
        {
            return JsonSerializer.Serialize(dto, HttpJson.Options);
        }

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        private class Dto
        {
            public VectorDto Vector { get; init; } = null!;
        }

        private record RecordDto(VectorDto Vector);
    }
}
