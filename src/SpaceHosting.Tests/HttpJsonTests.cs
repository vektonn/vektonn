using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using SpaceHosting.Contracts.ApiModels;
using SpaceHosting.Contracts.Json;

namespace SpaceHosting.Tests
{
    public class HttpJsonTests
    {
        [Test]
        public void DtoWithDenseVector()
        {
            var denseVector = RandomHelpers.NextDenseVector(dimension: 7).ToVectorDto();
            SerializeAndDeserializeDtoWithVector(new Dto {Vector = denseVector});
            SerializeAndDeserializeDtoWithVector(new RecordDto(denseVector));
        }

        [Test]
        public void DtoWithSparseVector()
        {
            var sparseVector = RandomHelpers.NextSparseVector(dimension: 100, coordinatesCount: 5).ToVectorDto();
            SerializeAndDeserializeDtoWithVector(new Dto {Vector = sparseVector});
            SerializeAndDeserializeDtoWithVector(new RecordDto(sparseVector));
        }

        private static void SerializeAndDeserializeDtoWithVector<TDto>(TDto dto)
        {
            Console.Out.WriteLine(dto.ToPrettyJson());

            var httpJson = JsonSerializer.Serialize(dto, HttpJson.Options);
            Console.Out.WriteLine(httpJson);

            var deserialized = JsonSerializer.Deserialize<TDto>(httpJson, HttpJson.Options);

            deserialized.Should().BeEquivalentTo(dto);
        }

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        private class Dto
        {
            public VectorDto Vector { get; init; } = null!;
        }

        private record RecordDto(VectorDto Vector);
    }
}
