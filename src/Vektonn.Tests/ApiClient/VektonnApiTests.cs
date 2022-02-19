using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vektonn.ApiClient;
using Vektonn.ApiContracts;
using Vektonn.SharedImpl.ApiContracts;
using Vektonn.SharedImpl.Contracts;
using Vektonn.SharedImpl.Json;
using Vostok.Logging.Console;

namespace Vektonn.Tests.ApiClient
{
    [Explicit]
    public class VektonnApiTests
    {
        private const int Dimension = 2;

        private readonly IndexId indexId = new(Name: "QuickStart.Index", Version: "1.0");

        private readonly VektonnApiClient vektonnApiClient = new(
            baseUri: new Uri("http://localhost:8081"),
            log: new SynchronousConsoleLog(),
            defaultRequestTimeout: TimeSpan.FromSeconds(1));

        [Test]
        public async Task Search()
        {
            var queryVectors = new[]
            {
                RandomHelpers.NextDenseVector(Dimension).ToVectorDto()!,
                RandomHelpers.NextDenseVector(Dimension).ToVectorDto()!
            };

            const int k = 3;
            var searchQuery = new SearchQueryDto(
                SplitFilter: null,
                QueryVectors: queryVectors,
                K: k,
                RetrieveVectors: true);

            var searchResult = await vektonnApiClient.SearchAsync(indexId.Name, indexId.Version, searchQuery);
            await Console.Out.WriteLineAsync(searchResult.ToPrettyJson());

            searchResult.Length.Should().Be(queryVectors.Length);
            searchResult[0].QueryVector.Should().BeEquivalentTo(queryVectors[0]);
            searchResult[0].NearestDataPoints.Length.Should().Be(0);
            searchResult[1].QueryVector.Should().BeEquivalentTo(queryVectors[1]);
            searchResult[1].NearestDataPoints.Length.Should().Be(0);
        }
    }
}
