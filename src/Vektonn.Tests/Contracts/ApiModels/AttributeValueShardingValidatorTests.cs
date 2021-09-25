using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Vektonn.Contracts;
using Vektonn.Contracts.ApiModels;
using Vektonn.Contracts.ApiModels.Validation;
using Vektonn.Contracts.Sharding;
using Vektonn.Contracts.Sharding.DataSource;
using static Vektonn.Tests.Contracts.ApiModels.AttributeDtoTestHelpers;

namespace Vektonn.Tests.Contracts.ApiModels
{
    public class AttributeValueShardingValidatorTests
    {
        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(42)]
        public void Validate_HashBasedSharding(int value)
        {
            const string key = "key";

            var sut = new AttributeValueShardingValidator(
                new Dictionary<string, IDataSourceAttributeValueSharder>
                {
                    {key, new HashBasedDataSourceAttributeValueSharder(1, new AttributeValueHasher())}
                },
                errorMessagePrefix: "Test");

            sut.Validate(Attribute(key, value)).IsValid.Should().BeTrue();
        }

        [TestCaseSource(nameof(TestCases))]
        public string Validate_ValueBasedSharding(AttributeDto attribute, (string Key, AttributeValue[] PossibleValues)[] shardingMeta)
        {
            var shardersByAttributeKey = shardingMeta.ToDictionary(
                t => t.Key,
                t => (IDataSourceAttributeValueSharder)new ValueBasedDataSourceAttributeValueSharder(new AttributeValueHasher(), possibleValues: t.PossibleValues.ToHashSet())
            );

            var sut = new AttributeValueShardingValidator(shardersByAttributeKey, errorMessagePrefix: "Test");

            return sut.Validate(attribute).ToString(separator: " \n ");
        }

        private static IEnumerable<TestCaseData> TestCases()
        {
            yield return new TestCaseData(
                Attribute("key", value: true),
                ValueBasedSharders(("key", PossibleValues(false)))
            ) {ExpectedResult = "Test attribute 'key' has unacceptable value 'True' for corresponding sharder"};

            yield return new TestCaseData(
                Attribute("key", value: false),
                ValueBasedSharders(("key", PossibleValues(true)))
            ) {ExpectedResult = "Test attribute 'key' has unacceptable value 'False' for corresponding sharder"};

            yield return new TestCaseData(
                Attribute("key", value: 42),
                ValueBasedSharders(("key", PossibleValues(Array.Empty<long>())))
            ) {ExpectedResult = "Test attribute 'key' has unacceptable value '42' for corresponding sharder"};

            yield return new TestCaseData(
                Attribute("key", value: 42),
                ValueBasedSharders(("key", PossibleValues(41, 43)), ("otherKey", PossibleValues(42)))
            ) {ExpectedResult = "Test attribute 'key' has unacceptable value '42' for corresponding sharder"};

            yield return new TestCaseData(
                Attribute("key", value: 42),
                ValueBasedSharders(("KEY", PossibleValues(42)))
            ) {ExpectedResult = string.Empty};

            yield return new TestCaseData(
                Attribute("key", value: 42),
                ValueBasedSharders()
            ) {ExpectedResult = string.Empty};

            yield return new TestCaseData(
                Attribute("key", value: 42),
                ValueBasedSharders(("otherKey", PossibleValues(0, 1)))
            ) {ExpectedResult = string.Empty};

            yield return new TestCaseData(
                Attribute("key", value: 42),
                ValueBasedSharders(("key", PossibleValues(0, 1, 42)))
            ) {ExpectedResult = string.Empty};
        }

        private static AttributeValue[] PossibleValues(params bool[] possibleValues)
        {
            return possibleValues.Select(x => new AttributeValue(Bool: x)).ToArray();
        }

        private static AttributeValue[] PossibleValues(params long[] possibleValues)
        {
            return possibleValues.Select(x => new AttributeValue(Int64: x)).ToArray();
        }

        private static (string Key, AttributeValue[] PossibleValues)[] ValueBasedSharders(params (string Key, AttributeValue[] PossibleValues)[] shardingMeta)
        {
            return shardingMeta;
        }
    }
}
