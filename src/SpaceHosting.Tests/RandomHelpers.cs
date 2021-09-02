using System;
using System.Collections.Generic;
using System.Linq;
using SpaceHosting.Index;

namespace SpaceHosting.Tests
{
    public static class RandomHelpers
    {
        private static readonly Random Random = new Random(42);

        public static DenseVector NextDenseVector(int dimension)
        {
            return new DenseVector(NextCoordinates(dimension));
        }

        public static SparseVector NextSparseVector(int dimension, int coordinatesCount)
        {
            return new SparseVector(dimension, NextCoordinates(coordinatesCount), NextDistinctIndices(coordinatesCount, dimension));
        }

        public static double[] NextCoordinates(int count)
        {
            return Enumerable.Range(0, count).Select(i => Random.NextDouble() * (i % 3 == 0 ? -1 : 1)).ToArray();
        }

        public static int[] NextDistinctIndices(int count, int dimension)
        {
            return NextIndices(100 * count, dimension).Distinct().Take(count).ToArray();
        }

        public static IEnumerable<int> NextIndices(int count, int dimension)
        {
            return Enumerable.Range(0, count).Select(_ => Random.Next(dimension));
        }
    }
}
