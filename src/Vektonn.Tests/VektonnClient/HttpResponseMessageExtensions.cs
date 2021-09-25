using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Vektonn.SharedImpl.Json;

namespace Vektonn.Tests.VektonnClient
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<TResponse> DeserializeJsonResponseAsync<TResponse>(this HttpResponseMessage responseMessage)
        {
            var responseString = await responseMessage.Content.ReadAsStringAsync();
            var responseModel = JsonSerializer.Deserialize<TResponse>(responseString, HttpJson.Options);
            return responseModel ?? throw new InvalidOperationException("Got null response");
        }
    }
}
