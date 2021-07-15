using System;
using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using SpaceHosting.ApiModels;
using SpaceHosting.Index;
using SpaceHosting.Json;

namespace SpaceHosting.Tests
{
    public class HttpJsonTests
    {
        [Test]
        public void DtoWithDenseVector()
        {
            var denseVector = RandomHelpers.NextDenseVector(dimension: 7);
            SerializeAndDeserializeDtoWithVector(new Dto {Vector = denseVector.ToVectorDto()});
            SerializeAndDeserializeDtoWithVector(new DtoWithIVector {Vector = denseVector});
        }

        [Test]
        public void DtoWithSparseVector()
        {
            var sparseVector = RandomHelpers.NextSparseVector(dimension: 100, coordinatesCount: 5);
            SerializeAndDeserializeDtoWithVector(new Dto {Vector = sparseVector.ToVectorDto()});
            SerializeAndDeserializeDtoWithVector(new DtoWithIVector {Vector = sparseVector});
        }

        private static void SerializeAndDeserializeDtoWithVector<TDto>(TDto dto)
        {
            Console.Out.WriteLine(dto.ToPrettyJson());

            var httpJson = JsonSerializer.Serialize(dto, HttpJson.Options);
            Console.Out.WriteLine(httpJson);

            var deserialized = JsonSerializer.Deserialize<TDto>(httpJson, HttpJson.Options);

            deserialized.Should().BeEquivalentTo(dto);
        }

        private class Dto
        {
            public VectorDto Vector { get; init; } = null!;
        }

        private class DtoWithIVector
        {
            public IVector Vector { get; init; } = null!;
        }
    }
}
