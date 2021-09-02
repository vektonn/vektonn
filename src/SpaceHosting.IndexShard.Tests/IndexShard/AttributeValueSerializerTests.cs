using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SpaceHosting.IndexShard.Models;
using SpaceHosting.IndexShard.Service.Shard;

namespace SpaceHosting.IndexShard.Tests.IndexShard
{
    public class AttributeValueSerializerTests
    {
        [TestCaseSource(nameof(BufferSizeTestCases))]
        public int Serialize_Deserialize_And_AssertBufferSize(AttributeValue[] values)
        {
            var bytes = AttributeValueSerializer.Serialize(values);

            AttributeValueSerializer
                .Deserialize(bytes)
                .Should()
                .BeEquivalentTo(values, options => options.WithStrictOrdering());

            return bytes.Length;
        }

        private static IEnumerable<TestCaseData> BufferSizeTestCases()
        {
            yield return new TestCaseData(arg: new AttributeValue[] {}) {TestName = "{m}([])", ExpectedResult = 0};

            yield return new TestCaseData(arg: new[] {new AttributeValue()}) {TestName = "{m}([EmptyAttribute])", ExpectedResult = 2};

            yield return new TestCaseData(arg: new[] {new AttributeValue(Bool: false)}) {ExpectedResult = 4};
            yield return new TestCaseData(arg: new[] {new AttributeValue(Bool: true)}) {ExpectedResult = 4};

            yield return new TestCaseData(arg: new[] {new AttributeValue(Guid: Guid.Empty)}) {ExpectedResult = 4};
            yield return new TestCaseData(arg: new[] {new AttributeValue(Guid: Guid.NewGuid())}) {ExpectedResult = 20};

            yield return new TestCaseData(arg: new[] {new AttributeValue(String: string.Empty)}) {TestName = "{m}([EmptyString])", ExpectedResult = 4};
            yield return new TestCaseData(arg: new[] {new AttributeValue(String: "x")}) {ExpectedResult = 5};
            yield return new TestCaseData(arg: new[] {new AttributeValue(String: "xy")}) {ExpectedResult = 6};
            yield return new TestCaseData(arg: new[] {new AttributeValue(String: "xyz")}) {ExpectedResult = 7};
            yield return new TestCaseData(arg: new[] {new AttributeValue(String: Guid.NewGuid().ToString("N"))}) {ExpectedResult = 36};

            yield return new TestCaseData(arg: new[] {new AttributeValue(Int64: 0L)}) {ExpectedResult = 4};
            yield return new TestCaseData(arg: new[] {new AttributeValue(Int64: byte.MaxValue)}) {ExpectedResult = 5};
            yield return new TestCaseData(arg: new[] {new AttributeValue(Int64: short.MaxValue)}) {ExpectedResult = 6};
            yield return new TestCaseData(arg: new[] {new AttributeValue(Int64: int.MaxValue)}) {ExpectedResult = 8};
            yield return new TestCaseData(arg: new[] {new AttributeValue(Int64: long.MaxValue)}) {ExpectedResult = 12};
            yield return new TestCaseData(arg: new[] {new AttributeValue(Int64: long.MinValue)}) {ExpectedResult = 13};

            yield return new TestCaseData(arg: new[] {new AttributeValue(DateTime: new DateTime(2021, 07, 31, 13, 52, 59, DateTimeKind.Utc))}) {ExpectedResult = 10};

            yield return new TestCaseData(arg: new[] {new AttributeValue(Guid: Guid.NewGuid()), new AttributeValue(Guid: Guid.NewGuid())}) {ExpectedResult = 40};
            yield return new TestCaseData(arg: new[] {new AttributeValue(Guid: Guid.NewGuid()), new AttributeValue(Bool: true)}) {ExpectedResult = 24};
            yield return new TestCaseData(arg: new[] {new AttributeValue(Guid: Guid.NewGuid()), new AttributeValue(Int64: long.MaxValue)}) {ExpectedResult = 32};
            yield return new TestCaseData(arg: new[] {new AttributeValue(Guid: Guid.NewGuid()), new AttributeValue(String: "la-la-la")}) {ExpectedResult = 32};
            yield return new TestCaseData(arg: new[] {new AttributeValue(Guid: Guid.NewGuid()), new AttributeValue(DateTime: new DateTime(2021, 07, 31, 13, 52, 59, DateTimeKind.Utc))}) {ExpectedResult = 30};
        }
    }
}
