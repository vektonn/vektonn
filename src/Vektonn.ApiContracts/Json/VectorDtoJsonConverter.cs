using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vektonn.ApiContracts.Json
{
    public class VectorDtoJsonConverter : JsonConverter<VectorDto>
    {
        private const string IsSparseVectorPropName = "isSparse";
        private const string CoordinatesPropName = "coordinates";
        private const string CoordinateIndicesPropName = "coordinateIndices";

        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(VectorDto).IsAssignableFrom(typeToConvert);
        }

        public override VectorDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            var isSparse = ReadIsSparseFlag(ref reader);
            var coordinates = ReadCoordinates(ref reader, options);

            VectorDto vectorDto;
            if (isSparse)
            {
                var coordinateIndices = ReadCoordinateIndices(ref reader, options);
                vectorDto = new SparseVectorDto(coordinates, coordinateIndices);
            }
            else
            {
                vectorDto = new DenseVectorDto(coordinates);
            }

            reader.Read();
            if (reader.TokenType != JsonTokenType.EndObject)
                throw new JsonException();

            return vectorDto;
        }

        public sealed override void Write(Utf8JsonWriter writer, VectorDto vectorDto, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            switch (vectorDto)
            {
                case DenseVectorDto denseVectorDto:
                    writer.WriteBoolean(IsSparseVectorPropName, false);
                    WriteCoordinates(writer, denseVectorDto.Coordinates);
                    break;
                case SparseVectorDto sparseVectorDto:
                    writer.WriteBoolean(IsSparseVectorPropName, true);
                    WriteCoordinates(writer, sparseVectorDto.Coordinates);
                    WriteCoordinateIndices(writer, sparseVectorDto.CoordinateIndices);
                    break;
                default:
                    throw new InvalidOperationException($"Invalid vector type: {vectorDto.GetType()}");
            }

            writer.WriteEndObject();
        }

        private static bool ReadIsSparseFlag(ref Utf8JsonReader reader)
        {
            ReadPropertyName(ref reader, IsSparseVectorPropName);
            reader.Read();
            return reader.GetBoolean();
        }

        private static double[] ReadCoordinates(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            ReadPropertyName(ref reader, CoordinatesPropName);
            var coordinates = JsonSerializer.Deserialize<double[]>(ref reader, options);
            return coordinates ?? throw new JsonException();
        }

        private static int[] ReadCoordinateIndices(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            ReadPropertyName(ref reader, CoordinateIndicesPropName);
            var coordinateIndices = JsonSerializer.Deserialize<int[]>(ref reader, options);
            return coordinateIndices ?? throw new JsonException();
        }

        private static void ReadPropertyName(ref Utf8JsonReader reader, string expectedPropName)
        {
            reader.Read();
            if (reader.TokenType != JsonTokenType.PropertyName || !string.Equals(reader.GetString(), expectedPropName, StringComparison.OrdinalIgnoreCase))
                throw new JsonException($"Expected property name: {expectedPropName}");
        }

        private static void WriteCoordinates(Utf8JsonWriter writer, double[] coordinates)
        {
            writer.WriteStartArray(CoordinatesPropName);
            foreach (var coordinate in coordinates)
                writer.WriteNumberValue(coordinate);
            writer.WriteEndArray();
        }

        private static void WriteCoordinateIndices(Utf8JsonWriter writer, int[] coordinateIndices)
        {
            writer.WriteStartArray(CoordinateIndicesPropName);
            foreach (var index in coordinateIndices)
                writer.WriteNumberValue(index);
            writer.WriteEndArray();
        }
    }
}
