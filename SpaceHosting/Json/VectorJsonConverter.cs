using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using SpaceHosting.Index;

namespace SpaceHosting.Json
{
    public class VectorJsonConverter : JsonConverter<IVector>
    {
        private const string IsSparseVectorPropName = "isSparse";
        private const string DimensionPropName = "dimension";
        private const string CoordinatesPropName = "coordinates";
        private const string CoordinateIndicesPropName = "coordinateIndices";

        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(IVector).IsAssignableFrom(typeToConvert);
        }

        public override IVector Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            var isSparse = ReadIsSparseFlag(ref reader);
            var dimension = ReadDimension(ref reader);
            var coordinates = ReadCoordinates(ref reader, options);

            IVector vector;
            if (isSparse)
            {
                var coordinateIndices = ReadCoordinateIndices(ref reader, options);
                vector = new SparseVector(dimension, coordinateIndices, coordinates);
            }
            else
            {
                vector = new DenseVector(coordinates);
            }

            reader.Read();
            if (reader.TokenType != JsonTokenType.EndObject)
                throw new JsonException();

            return vector;
        }

        public sealed override void Write(Utf8JsonWriter writer, IVector vector, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            switch (vector)
            {
                case DenseVector denseVector:
                    writer.WriteBoolean(IsSparseVectorPropName, false);
                    writer.WriteNumber(DimensionPropName, denseVector.Dimension);
                    WriteCoordinates(writer, denseVector.Coordinates);
                    break;
                case SparseVector sparseVector:
                    writer.WriteBoolean(IsSparseVectorPropName, true);
                    writer.WriteNumber(DimensionPropName, sparseVector.Dimension);
                    WriteCoordinates(writer, sparseVector.Coordinates);
                    WriteCoordinateIndices(writer, sparseVector.ColumnIndices);
                    break;
                default:
                    throw new InvalidOperationException($"Invalid vector type: {vector.GetType()}");
            }

            writer.WriteEndObject();
        }

        private static bool ReadIsSparseFlag(ref Utf8JsonReader reader)
        {
            ReadPropertyName(ref reader, IsSparseVectorPropName);
            reader.Read();
            return reader.GetBoolean();
        }

        private static int ReadDimension(ref Utf8JsonReader reader)
        {
            ReadPropertyName(ref reader, DimensionPropName);
            reader.Read();
            return reader.GetInt32();
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
