using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Vektonn.Contracts.ApiModels;
using Vektonn.SharedImpl.Json;

namespace Vektonn.Tests.VektonnClient
{
    public class VektonnHttpClient
    {
        private readonly HttpClient httpClient;

        public VektonnHttpClient(Uri baseUri)
        {
            httpClient = new HttpClient {BaseAddress = baseUri};
        }

        public async Task<IndexInfoDto> GetIndexInfoAsync()
        {
            var responseMessage = await httpClient.GetAsync("api/v1/info");
            responseMessage.EnsureSuccessStatusCode();

            return await responseMessage.DeserializeJsonResponseAsync<IndexInfoDto>();
        }

        public async Task<SearchResultDto[]> SearchAsync(SearchQueryDto searchQuery)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri: "api/v1/search")
            {
                Content = JsonContent.Create(searchQuery, options: HttpJson.Options)
            };

            var responseMessage = await httpClient.SendAsync(requestMessage);
            responseMessage.EnsureSuccessStatusCode();

            return await responseMessage.DeserializeJsonResponseAsync<SearchResultDto[]>();
        }
    }
}
