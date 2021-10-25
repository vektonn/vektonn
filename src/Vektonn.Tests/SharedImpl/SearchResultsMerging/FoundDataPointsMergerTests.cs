using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using MoreLinq;
using NUnit.Framework;
using Vektonn.SharedImpl.SearchResultsMerging;

namespace Vektonn.Tests.SharedImpl.SearchResultsMerging
{
    public class FoundDataPointsMergerTests
    {
        private readonly Random random = new Random(42);
        private readonly IComparer<TestFoundDataPoint> foundDataPointComparer = new ByPropertyComparer<TestFoundDataPoint, double>(x => x.Distance);

        [Test]
        public void SingleResultGoesToFinalResultAsIs()
        {
            const int limit = 2;
            const int queryVectorsCount = 3;
            var results = GenerateResults(queryVectorsCount, limit);
            FoundDataPointsMerger.Merge(new[] {results}, limit, ListSortDirection.Ascending, foundDataPointComparer)
                .Should()
                .BeEquivalentTo(results, options => options.WithStrictOrdering());
        }

        [Test]
        public void ResultMergesWithEmptyResult()
        {
            const int limit = 2;
            const int queryVectorsCount = 3;
            var results = GenerateResults(queryVectorsCount, limit);
            var emptyResults = GenerateResults(queryVectorsCount, 0);
            FoundDataPointsMerger.Merge(new[] {results, emptyResults}, limit, ListSortDirection.Ascending, foundDataPointComparer)
                .Should()
                .BeEquivalentTo(results, options => options.WithStrictOrdering());
        }

        [TestCase(1)]
        [TestCase(3)]
        public void MergeResults_TrimsResultsToLimit(int shardsCount)
        {
            const int limit = 2;
            const int queryVectorsCount = 4;
            var resultsArrays = GenerateManyResults(queryVectorsCount, limit * 2, shardsCount);
            FoundDataPointsMerger.Merge(resultsArrays, limit, ListSortDirection.Ascending, foundDataPointComparer)
                .ForEach(
                    r => r
                        .Should()
                        .HaveCountLessOrEqualTo(limit));
        }

        [Test]
        public void MergeResults_GeneratesSortedResults()
        {
            const int limit = 2;
            const int queryVectorsCount = 4;
            var resultsArrays = GenerateManyResults(queryVectorsCount, limit * 2, 3);
            FoundDataPointsMerger.Merge(resultsArrays, limit, ListSortDirection.Ascending, foundDataPointComparer)
                .ForEach(
                    r => r
                        .Should()
                        .BeInAscendingOrder(x => x.Distance));
        }

        [TestCase(OrderByDirection.Ascending)]
        [TestCase(OrderByDirection.Descending)]
        public void MergeResults_EqualsToSlowMerge(ListSortDirection mergeSortDirection)
        {
            const int limit = 2;
            const int queryVectorsCount = 4;
            var resultsArrays = GenerateManyResults(queryVectorsCount, resultLength: limit * 2, shardsCount: 3, mergeSortDirection).ToArray();
            var slowMerge = SlowMerge(queryVectorsCount, limit, resultsArrays, mergeSortDirection);
            var quickMerge = FoundDataPointsMerger.Merge(resultsArrays, limit, mergeSortDirection, foundDataPointComparer);
            quickMerge
                .Should()
                .BeEquivalentTo(slowMerge, options => options.WithStrictOrdering());
        }

        private TestFoundDataPoint GenerateFoundDataPoint()
        {
            return new TestFoundDataPoint
            {
                Id = Guid.NewGuid(),
                Distance = random.NextDouble()
            };
        }

        private TestFoundDataPoint[] GenerateResult(int resultLength, ListSortDirection mergeSortDirection)
        {
            var results = Enumerable
                .Range(0, resultLength)
                .Select(_ => GenerateFoundDataPoint());

            return Sort(results, mergeSortDirection).ToArray();
        }

        private TestFoundDataPoint[][] GenerateResults(int count, int resultLength, ListSortDirection mergeSortDirection = ListSortDirection.Ascending)
        {
            return Enumerable.Range(0, count)
                .Select(_ => GenerateResult(resultLength, mergeSortDirection))
                .ToArray();
        }

        private IEnumerable<TestFoundDataPoint[][]> GenerateManyResults(int count, int resultLength, int shardsCount, ListSortDirection mergeSortDirection = ListSortDirection.Ascending)
        {
            return Enumerable
                .Range(0, shardsCount)
                .Select(_ => GenerateResults(count, resultLength, mergeSortDirection));
        }

        private static TestFoundDataPoint[][] SlowMerge(int queryVectorsCount, int limit, TestFoundDataPoint[][][] responses, ListSortDirection mergeSortDirection)
        {
            var merged = new List<TestFoundDataPoint[]>();

            for (var i = 0; i < queryVectorsCount; i++)
            {
                var allDataPoints = responses.SelectMany(x => x[i]).ToArray(); // concat all shard results for each query vector

                var sortedDataPoints = Sort(allDataPoints, mergeSortDirection);

                var mergedFoundDataPoints = sortedDataPoints
                    .Take(limit) // take first limit results for each query vector
                    .ToArray();

                merged.Add(mergedFoundDataPoints);
            }

            return merged.ToArray();
        }

        private static IEnumerable<TestFoundDataPoint> Sort(IEnumerable<TestFoundDataPoint> dataPoints, ListSortDirection mergeSortDirection)
        {
            return mergeSortDirection switch
            {
                ListSortDirection.Ascending => dataPoints.OrderBy(x => x.Distance),
                ListSortDirection.Descending => dataPoints.OrderByDescending(x => x.Distance),
                _ => throw new InvalidOperationException($"Invalid mergeSortDirection: {mergeSortDirection}")
            };
        }

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        private class TestFoundDataPoint
        {
            public Guid Id { get; init; }
            public double Distance { get; init; }
        }
    }
}
