using System.Text.Json;
using Vektonn.ApiContracts.Json;

namespace Vektonn.SharedImpl.Json
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
            Options.Converters.Add(new VectorDtoJsonConverter());
        }

        public static string ToPrettyJson<T>(this T obj)
        {
            return JsonSerializer.Serialize(obj, Options);
        }
    }
}
