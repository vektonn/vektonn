using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SpaceHosting.Contracts.ApiModels;
using SpaceHosting.Contracts.Json;
using SpaceHosting.Tests.SpaceHostingClient;

namespace SpaceHosting.Tests
{
    [Explicit]
    public class ApiTests
    {
        private const int Dimension = 32;
        private readonly SpaceHostingHttpClient httpClient = new SpaceHostingHttpClient(new Uri("http://localhost:8080"));

        [Test]
        public async Task IndexInfo()
        {
            var indexInfo = await httpClient.GetIndexInfoAsync();
            await Console.Out.WriteLineAsync(indexInfo.ToPrettyJson());

            indexInfo.VectorDimension.Should().Be(Dimension);
            indexInfo.DataPointsCount.Should().Be(32);
        }

        [Test]
        public async Task SearchDense()
        {
            var queryVectors = new[]
            {
                RandomHelpers.NextDenseVector(Dimension).ToVectorDto(),
                RandomHelpers.NextDenseVector(Dimension).ToVectorDto()
            };

            await TestSearchAsync(k: 3, queryVectors);
        }

        [Test]
        public async Task SearchSparse()
        {
            var queryVectors = new[]
            {
                RandomHelpers.NextSparseVector(Dimension, 5).ToVectorDto(),
                RandomHelpers.NextSparseVector(Dimension, 7).ToVectorDto()
            };

            await TestSearchAsync(k: 3, queryVectors);
        }

        private async Task TestSearchAsync(int k, VectorDto[] queryVectors)
        {
            var searchQuery = new SearchQueryDto(
                SplitFilter: null,
                QueryVectors: queryVectors,
                K: k);

            var searchResult = await httpClient.SearchAsync(searchQuery);
            await Console.Out.WriteLineAsync(searchResult.ToPrettyJson());

            searchResult.Length.Should().Be(queryVectors.Length);
            searchResult[0].QueryVector.Should().BeEquivalentTo(queryVectors[0]);
            searchResult[0].NearestDataPoints.Length.Should().Be(k);
            searchResult[1].QueryVector.Should().BeEquivalentTo(queryVectors[1]);
            searchResult[1].NearestDataPoints.Length.Should().Be(k);
        }
    }
}
