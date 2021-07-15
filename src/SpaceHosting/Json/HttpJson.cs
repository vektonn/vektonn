using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpaceHosting.Json
{
    public static class HttpJson
    {
        public static readonly JsonSerializerOptions Options;

        static HttpJson()
        {
            Options = new JsonSerializerOptions
            {
                MaxDepth = 32,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            };
            Options.Converters.Add(new VectorJsonConverter());
            Options.Converters.Add(new VectorDtoJsonConverter());
        }

        public static void Configure(JsonSerializerOptions options)
        {
            options.MaxDepth = Options.MaxDepth;
            options.NumberHandling = Options.NumberHandling;
            options.PropertyNamingPolicy = Options.PropertyNamingPolicy;
            options.PropertyNameCaseInsensitive = Options.PropertyNameCaseInsensitive;
            options.DefaultIgnoreCondition = Options.DefaultIgnoreCondition;
            options.Converters.Add(new VectorJsonConverter());
            options.Converters.Add(new VectorDtoJsonConverter());
        }
    }
}
