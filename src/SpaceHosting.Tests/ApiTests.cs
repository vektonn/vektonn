using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SpaceHosting.ApiModels;
using SpaceHosting.Index;
using SpaceHosting.Json;
using SpaceHosting.Tests.SpaceHostingClient;

namespace SpaceHosting.Tests
{
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
            indexInfo.VectorCount.Should().Be(11530);
        }

        [Test]
        public async Task SearchDense()
        {
            var queryVectors = new IVector[]
            {
                RandomHelpers.NextDenseVector(Dimension),
                RandomHelpers.NextDenseVector(Dimension)
            };

            await TestSearchAsync(k: 3, queryVectors);
        }

        [Test]
        public async Task SearchSparse()
        {
            var queryVectors = new IVector[]
            {
                RandomHelpers.NextSparseVector(Dimension, 5),
                RandomHelpers.NextSparseVector(Dimension, 7)
            };

            await TestSearchAsync(k: 3, queryVectors);
        }

        private async Task TestSearchAsync(int k, IVector[] queryVectors)
        {
            var searchQuery = new SearchQueryDto
            {
                K = k,
                Vectors = queryVectors.ToVectorDto()
            };

            var searchResult = await httpClient.SearchAsync(searchQuery);
            await Console.Out.WriteLineAsync(searchResult.ToPrettyJson());

            searchResult.Length.Should().Be(queryVectors.Length);
            searchResult[0].Length.Should().Be(k);
            searchResult[1].Length.Should().Be(k);
        }
    }
}
