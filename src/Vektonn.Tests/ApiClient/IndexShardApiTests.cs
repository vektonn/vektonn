using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vektonn.ApiClient.IndexShard;
using Vektonn.ApiContracts;
using Vektonn.SharedImpl.ApiContracts;
using Vektonn.SharedImpl.Json;
using Vostok.Logging.Console;
using Vostok.Tracing.Abstractions;

namespace Vektonn.Tests.ApiClient
{
    [Explicit]
    public class IndexShardApiTests
    {
        private const int Dimension = 2;

        private readonly DenseVectorDto denseZeroVector = new(
            Coordinates: new double[Dimension]);

        private readonly SparseVectorDto sparseZeroVector = new(
            Coordinates: Array.Empty<double>(),
            CoordinateIndices: Array.Empty<int>());

        private readonly IndexShardApiClient indexShardApiClient = new(
            new IndexShardApiAbsoluteUriClusterClient(new SynchronousConsoleLog(), new DevNullTracer()),
            indexShardBaseUri: new Uri("http://localhost:8082"));

        [Test]
        public async Task IndexInfo()
        {
            var indexInfo = await indexShardApiClient.GetIndexInfoAsync();
            await Console.Out.WriteLineAsync(indexInfo.ToPrettyJson());

            indexInfo.VectorDimension.Should().Be(Dimension);
            indexInfo.DataPointsCount.Should().Be(0);
        }

        [Test]
        public async Task Probe()
        {
            var searchResult = await indexShardApiClient.ProbeAsync();
            await Console.Out.WriteLineAsync(searchResult.ToPrettyJson());

            searchResult.QueryVector.Should().BeEquivalentTo(denseZeroVector);
            searchResult.NearestDataPoints.Should().BeEmpty();
        }

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

            var searchResult = await indexShardApiClient.SearchAsync(searchQuery);
            await Console.Out.WriteLineAsync(searchResult.ToPrettyJson());

            searchResult.Length.Should().Be(queryVectors.Length);
            searchResult[0].QueryVector.Should().BeEquivalentTo(queryVectors[0]);
            searchResult[0].NearestDataPoints.Length.Should().Be(0);
            searchResult[1].QueryVector.Should().BeEquivalentTo(queryVectors[1]);
            searchResult[1].NearestDataPoints.Length.Should().Be(0);
        }
    }
}
