using System;
using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using SpaceHosting.Index;
using SpaceHosting.Json;

namespace SpaceHosting.Tests
{
    public class VectorJsonConverterTests
    {
        [Test]
        public void DtoWithDenseVector()
        {
            var denseVector = RandomHelpers.NextDenseVector(dimension: 7);
            SerializeAndDeserializeDtoWithVector(denseVector);
        }

        [Test]
        public void DtoWithSparseVector()
        {
            var sparseVector = RandomHelpers.NextSparseVector(dimension: 100, coordinatesCount: 5);
            SerializeAndDeserializeDtoWithVector(sparseVector);
        }

        private static void SerializeAndDeserializeDtoWithVector(IVector vector)
        {
            var dto = new Dto {Vector = vector};
            Console.Out.WriteLine(dto.ToPrettyJson());

            var json = JsonSerializer.Serialize(dto, HttpJson.Options);
            Console.Out.WriteLine(json);

            var deserialized = JsonSerializer.Deserialize<Dto>(json, HttpJson.Options);

            deserialized.Should().BeEquivalentTo(dto);
        }

        private class Dto
        {
            public IVector Vector { get; set; } = null!;
        }
    }
}
