using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MoreLinq;

namespace SpaceHosting.Contracts.ResultsMerging
{
    public static class FoundDataPointsMerger
    {
        public static IEnumerable<TFoundDataPoint[]> Merge<TFoundDataPoint>(
            IEnumerable<TFoundDataPoint[][]> results,
            int limit,
            ListSortDirection mergeSortDirection,
            IComparer<TFoundDataPoint> comparer)
            where TFoundDataPoint : class
        {
            return results
                .Aggregate((result1, result2) => MergeResults(result1, result2, limit, mergeSortDirection, comparer).ToArray())
                .Select(result => result.Take(limit).ToArray());
        }

        private static IEnumerable<TFoundDataPoint[]> MergeResults<TFoundDataPoint>(
            IReadOnlyList<TFoundDataPoint[]> result1,
            IReadOnlyList<TFoundDataPoint[]> result2,
            int limit,
            ListSortDirection mergeSortDirection,
            IComparer<TFoundDataPoint> comparer)
            where TFoundDataPoint : class
        {
            for (var i = 0; i < result1.Count; i++)
            {
                var res1 = result1[i];
                var res2 = result2[i];

                yield return res1
                    .SortedMerge(ToOrderByDirection(mergeSortDirection), comparer, res2)
                    .Take(limit)
                    .ToArray();
            }
        }

        private static OrderByDirection ToOrderByDirection(ListSortDirection mergeSortDirection) => mergeSortDirection switch
        {
            ListSortDirection.Ascending => OrderByDirection.Ascending,
            ListSortDirection.Descending => OrderByDirection.Descending,
            _ => throw new InvalidOperationException($"Invalid {nameof(mergeSortDirection)}: {mergeSortDirection}")
        };
    }
}
