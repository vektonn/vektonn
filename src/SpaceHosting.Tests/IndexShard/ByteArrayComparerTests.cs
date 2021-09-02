using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using FluentAssertions;
using NUnit.Framework;
using SpaceHosting.IndexShard.Shard;

namespace SpaceHosting.Tests.IndexShard
{
    public class ByteArrayComparerTests
    {
        private readonly byte[] emptyArray = {};
        private readonly byte[] a1 = {0xff};
        private readonly byte[] a2 = {0xff, 0x01};
        private readonly byte[] a3 = {0xff, 0x01, 0x02};
        private readonly byte[] a4 = {0xff, 0x01, 0x02, 0x03};
        private readonly byte[] a5 = {0xff, 0x01, 0x02, 0x03, 0x04};
        private readonly ByteArrayComparer comparer = new ByteArrayComparer();

        [Test]
        public void Equals_NullArgs()
        {
            Assert.IsTrue(comparer.Equals(null, null));
            Assert.IsFalse(comparer.Equals(emptyArray, null));
            Assert.IsFalse(comparer.Equals(null, emptyArray));
        }

        [Test]
        public void Equals_SameArrays()
        {
            Assert.IsTrue(comparer.Equals(emptyArray, emptyArray));
            Assert.IsTrue(comparer.Equals(a1, a1));
        }

        [Test]
        public void Equals_NonEqualArrays()
        {
            Assert.IsFalse(comparer.Equals(a1, emptyArray));
            Assert.IsFalse(comparer.Equals(a2, a1));
            Assert.IsFalse(comparer.Equals(a3, a2));
            Assert.IsFalse(comparer.Equals(a4, a3));
            Assert.IsFalse(comparer.Equals(a5, a4));
        }

        [Test]
        public void Equals_SanityCheck()
        {
            var items = GenerateRandomByteArrays();
            foreach (var x in items)
            {
                foreach (var y in items)
                {
                    var actualResult = comparer.Equals(x, y);
                    var expectedResult = ArraysAreEqualCanonicalImpl(x, y);
                    Assert.That(actualResult == expectedResult);
                }
            }
        }

        [Test]
        public void GetHashCode_NotEqual()
        {
            Assert.AreNotEqual(comparer.GetHashCode(emptyArray), comparer.GetHashCode(a1));
            Assert.AreNotEqual(comparer.GetHashCode(a1), comparer.GetHashCode(a2));
            Assert.AreNotEqual(comparer.GetHashCode(a2), comparer.GetHashCode(a3));
            Assert.AreNotEqual(comparer.GetHashCode(a3), comparer.GetHashCode(a4));
        }

        [Test]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public void GetHashCode_Equal()
        {
            Assert.AreEqual(0, comparer.GetHashCode(null));
            Assert.AreEqual(0, comparer.GetHashCode(new byte[] {}));
            Assert.AreEqual(397, comparer.GetHashCode(new byte[] {0x0}));
            Assert.AreEqual(315218, comparer.GetHashCode(new byte[] {0x0, 0x0}));
            Assert.AreEqual(370, comparer.GetHashCode(new byte[] {0xff}));
            Assert.AreEqual(395809, comparer.GetHashCode(new byte[] {0xff, 0x0}));
            Assert.AreNotEqual(comparer.GetHashCode(a4), comparer.GetHashCode(a5));
            Assert.AreEqual(comparer.GetHashCode(new byte[] {0x01, 0x0ff}), comparer.GetHashCode(new byte[] {0x01, 0x0ff}));
        }

        [Test]
        [Timeout(15_000)]
        public void GetHashCode_Perf()
        {
            const int iterations = 10_000_000;
            var samples = new byte[iterations][];
            for (var i = 0; i < iterations; i++)
                samples[i] = Guid.NewGuid().ToByteArray();

            var stopwatch = Stopwatch.StartNew();
            long result = 0;
            for (var i = 0; i < iterations; i++)
                result += comparer.GetHashCode(samples[i]);

            stopwatch.Stop();
            Console.Out.WriteLine($"elapsed {stopwatch.ElapsedMilliseconds} millis, {result}");

            stopwatch.ElapsedMilliseconds.Should().BeLessThan(500);
        }

        [Test]
        public void Compare_NullArgs()
        {
            Assert.That(comparer.Compare(null, null), Is.EqualTo(0));
            Assert.That(comparer.Compare(emptyArray, null), Is.EqualTo(1));
            Assert.That(comparer.Compare(null, emptyArray), Is.EqualTo(-1));
        }

        [Test]
        public void Compare_EqualArrays()
        {
            Assert.That(comparer.Compare(emptyArray, emptyArray), Is.EqualTo(0));
            Assert.That(comparer.Compare(a1, a1), Is.EqualTo(0));
            Assert.That(comparer.Compare(new byte[] {0x01, 0x0ff}, new byte[] {0x01, 0x0ff}), Is.EqualTo(0));
        }

        [Test]
        public void Compare_NonEqualArrays()
        {
            Assert.That(comparer.Compare(a1, emptyArray), Is.EqualTo(1));
            Assert.That(comparer.Compare(a2, a1), Is.EqualTo(1));
            Assert.That(comparer.Compare(a1, a2), Is.EqualTo(-1));
            Assert.That(comparer.Compare(a3, a2), Is.EqualTo(1));
            Assert.That(comparer.Compare(a4, a3), Is.EqualTo(1));
            Assert.That(comparer.Compare(a5, a4), Is.EqualTo(1));
        }

        [Test]
        public void Compare_SanityCheck()
        {
            var items = GenerateRandomByteArrays();
            foreach (var x in items)
            {
                foreach (var y in items)
                {
                    var actualResult = comparer.Compare(x, y);
                    var expectedResult = CompareArraysLexicographically(x, y);
                    Assert.That(actualResult == expectedResult);
                }
            }
        }

        [Test]
        [Timeout(10_000)]
        public void Compare_Perf()
        {
            const int iterations = 10_000;
            var samples = new byte[iterations][];
            for (var i = 0; i < iterations; i++)
                samples[i] = Guid.NewGuid().ToByteArray();

            var stopwatch = Stopwatch.StartNew();
            long result = 0;
            for (var i = 0; i < iterations; i++)
            {
                for (var j = i + 1; j < iterations; j++)
                    result += comparer.Compare(samples[i], samples[j]);
            }

            stopwatch.Stop();
            Console.Out.WriteLine($"elapsed {stopwatch.ElapsedMilliseconds} millis, {result}");
        }

        private static List<byte[]> GenerateRandomByteArrays()
        {
            var items = new List<byte[]>();
            var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            for (var idx = 0; idx < 1000; idx++)
            {
                var item = new byte[64];
                if (idx > 0 && idx % 5 == 0)
                    items[idx - 1].CopyTo(item, 0);
                else
                    rngCryptoServiceProvider.GetNonZeroBytes(item);
                items.Add(item);
            }

            return items;
        }

        private static bool ArraysAreEqualCanonicalImpl<T>(T[]? x, T[]? y)
        {
            if ((x == null) ^ (y == null))
                return false;
            if (ReferenceEquals(x, y))
                return true;
            if (x!.Length != y!.Length)
                return false;
            var comparer = EqualityComparer<T>.Default;
            for (var i = 0; i < x.Length; i++)
            {
                if (!comparer.Equals(x[i], y[i]))
                    return false;
            }

            return true;
        }

        private static int CompareArraysLexicographically<T>(T[]? x, T[]? y)
        {
            if (x == null)
                return y == null ? 0 : -1;
            if (y == null)
                return 1;
            if (x.Length < y.Length)
                return CompareArraysByPrefix(x, y, x.Length, -1);
            if (x.Length > y.Length)
                return CompareArraysByPrefix(x, y, y.Length, 1);
            return CompareArraysByPrefix(x, y, x.Length, 0);
        }

        private static int CompareArraysByPrefix<T>(T[] x, T[] y, int prefixLength, int resultIfPrefixesAreEqual)
        {
            var comparer = Comparer<T>.Default;
            for (var i = 0; i < prefixLength; i++)
            {
                var res = comparer.Compare(x[i], y[i]);
                if (res != 0)
                    return res;
            }

            return resultIfPrefixesAreEqual;
        }
    }
}
