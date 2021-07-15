using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using SpaceHosting.ApiModels;
using SpaceHosting.Index;

namespace SpaceHosting.Json
{
    public class VectorDtoJsonConverter : JsonConverter<VectorDto>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(VectorDto).IsAssignableFrom(typeToConvert);
        }

        public override VectorDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var vector = JsonSerializer.Deserialize<IVector>(ref reader, options) ?? throw new JsonException();
            return vector.ToVectorDto();
        }

        public sealed override void Write(Utf8JsonWriter writer, VectorDto vectorDto, JsonSerializerOptions options)
        {
            var vector = vectorDto.ToVector();
            JsonSerializer.Serialize(writer, vector, options);
        }
    }
}
