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
        private const int Dimension = 100500;

        private readonly IndexId indexId = new(Name: "Samples.SparseVectors", Version: "0.1");

        private readonly VektonnApiClient vektonnApiClient = new(
            baseUri: new Uri("http://localhost:8081"),
            log: new SynchronousConsoleLog(),
            defaultRequestTimeout: TimeSpan.FromSeconds(1));

        [Test]
        public async Task Search()
        {
            var queryVectors = new[]
            {
                RandomHelpers.NextSparseVector(Dimension, 5).ToVectorDto(),
                RandomHelpers.NextSparseVector(Dimension, 7).ToVectorDto()
            };

            const int k = 3;
            var searchQuery = new SearchQueryDto(
                SplitFilter: null,
                QueryVectors: queryVectors,
                K: k);

            var searchResult = await vektonnApiClient.SearchAsync(indexId.Name, indexId.Version, searchQuery);
            await Console.Out.WriteLineAsync(searchResult.ToPrettyJson());

            searchResult.Length.Should().Be(queryVectors.Length);
            searchResult[0].QueryVector.Should().BeEquivalentTo(queryVectors[0]);
            searchResult[0].NearestDataPoints.Length.Should().Be(k);
            searchResult[1].QueryVector.Should().BeEquivalentTo(queryVectors[1]);
            searchResult[1].NearestDataPoints.Length.Should().Be(k);
        }
    }
}
