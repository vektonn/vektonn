using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Vektonn.ApiContracts;
using Vektonn.SharedImpl.ApiContracts.Validation;
using Vektonn.SharedImpl.Contracts;
using static Vektonn.Tests.SharedImpl.ApiContracts.AttributeDtoTestHelpers;

namespace Vektonn.Tests.SharedImpl.ApiContracts
{
    public class AttributeValidatorTests
    {
        [TestCaseSource(nameof(TestCases))]
        public string Validate(AttributeDto attribute, (string Key, AttributeValueTypeCode Type)[] knownAttributes)
        {
            var sut = new AttributeValidator(
                knownAttributes.ToDictionary(t => t.Key, t => t.Type),
                errorMessagePrefix: "Test");

            return sut.Validate(attribute).ToString(separator: " \n ");
        }

        private static IEnumerable<TestCaseData> TestCases()
        {
            yield return new TestCaseData(
                Attribute(key: null!, value: 42),
                KnownAttributes(("k1", AttributeValueTypeCode.Int64))
            ) {ExpectedResult = "Test attribute key is null or whitespace"};

            yield return new TestCaseData(
                Attribute(key: string.Empty, value: 42),
                KnownAttributes((Key: string.Empty, AttributeValueTypeCode.Int64))
            ) {ExpectedResult = "Test attribute key is null or whitespace"};

            yield return new TestCaseData(
                Attribute(key: " ", value: 42),
                KnownAttributes((Key: " ", AttributeValueTypeCode.Int64))
            ) {ExpectedResult = "Test attribute key is null or whitespace"};

            yield return new TestCaseData(
                Attribute(key: Environment.NewLine, value: 42),
                KnownAttributes((Key: Environment.NewLine, AttributeValueTypeCode.Int64))
            ) {ExpectedResult = "Test attribute key is null or whitespace"};

            yield return new TestCaseData(
                Attribute("k1", value: true),
                KnownAttributes(("k0", AttributeValueTypeCode.Bool), ("k2", AttributeValueTypeCode.Bool))
            ) {ExpectedResult = "Test attribute key is unknown: 'k1'"};

            yield return new TestCaseData(
                Attribute("K1", value: true),
                KnownAttributes(("k1", AttributeValueTypeCode.Bool))
            ) {ExpectedResult = "Test attribute key is unknown: 'K1'"};

            yield return new TestCaseData(
                new AttributeDto("k1", new AttributeValueDto(String: null, Guid: null, Bool: null, Int64: null, Float64: null, DateTime: null)),
                KnownAttributes(("k1", AttributeValueTypeCode.String))
            ) {ExpectedResult = "Test attribute 'k1' has invalid value: ''"};

            yield return new TestCaseData(
                new AttributeDto("k1", new AttributeValueDto(String: "str", Int64: 42)),
                KnownAttributes(("k1", AttributeValueTypeCode.String))
            ) {ExpectedResult = "Test attribute 'k1' has invalid value: 'str'"};

            yield return new TestCaseData(
                Attribute("k1", value: null),
                KnownAttributes(("k1", AttributeValueTypeCode.String))
            ) {ExpectedResult = "Test attribute 'k1' has invalid value: ''"};

            yield return new TestCaseData(
                Attribute("k1", value: 42),
                KnownAttributes(("k1", AttributeValueTypeCode.String))
            ) {ExpectedResult = "Test attribute 'k1' has invalid value: '42'"};

            yield return new TestCaseData(
                Attribute("k1", value: "str"),
                KnownAttributes(("k1", AttributeValueTypeCode.Guid))
            ) {ExpectedResult = "Test attribute 'k1' has invalid value: 'str'"};

            yield return new TestCaseData(
                Attribute("k1", value: 42),
                KnownAttributes(("k1", AttributeValueTypeCode.Int64))
            ) {ExpectedResult = string.Empty};

            yield return new TestCaseData(
                Attribute("k1", value: Guid.NewGuid()),
                KnownAttributes(("k1", AttributeValueTypeCode.Guid))
            ) {ExpectedResult = string.Empty};

            yield return new TestCaseData(
                Attribute("k1", value: 3.1415926),
                KnownAttributes(("k1", AttributeValueTypeCode.Float64))
            ) {ExpectedResult = string.Empty};

            yield return new TestCaseData(
                Attribute("k1", value: DateTime.UtcNow),
                KnownAttributes(("k1", AttributeValueTypeCode.DateTime))
            ) {ExpectedResult = string.Empty};

            yield return new TestCaseData(
                Attribute("k1", value: true),
                KnownAttributes(("k1", AttributeValueTypeCode.Bool), ("k2", AttributeValueTypeCode.Guid))
            ) {ExpectedResult = string.Empty};

            yield return new TestCaseData(
                Attribute("k2", value: "la-la-la"),
                KnownAttributes(("k1", AttributeValueTypeCode.Int64), ("k2", AttributeValueTypeCode.String))
            ) {ExpectedResult = string.Empty};
        }
    }
}
