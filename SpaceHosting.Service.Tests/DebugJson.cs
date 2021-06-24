using System.Text.Json;
using SpaceHosting.Json;

namespace SpaceHosting.Service.Tests
{
    public static class DebugJson
    {
        private static readonly JsonSerializerOptions Options;

        static DebugJson()
        {
            Options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            Options.Converters.Add(new VectorJsonConverter());
        }

        public static string ToPrettyJson<T>(this T obj)
        {
            return JsonSerializer.Serialize(obj, Options);
        }
    }
}
