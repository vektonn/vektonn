using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Vektonn.ApiContracts;
using Vektonn.SharedImpl.ApiContracts.Validation;
using Vektonn.SharedImpl.Contracts;
using static Vektonn.Tests.SharedImpl.ApiContracts.AttributeDtoTestHelpers;

namespace Vektonn.Tests.SharedImpl.ApiContracts
{
    public class SplitFilterValidatorTests
    {
        [TestCaseSource(nameof(TestCases))]
        public string Validate(AttributeDto[] splitFilter, (string Key, AttributeValueTypeCode Type)[] splitAttributes)
        {
            var sut = new SplitFilterValidator(splitAttributes.ToDictionary(t => t.Key, t => t.Type));

            return sut.Validate(splitFilter).ToString(separator: " \n ");
        }

        private static IEnumerable<TestCaseData> TestCases()
        {
            yield return new TestCaseData(
                new[] {Attribute("sk3", value: 0), Attribute("sk2", value: 1), Attribute("sk1", value: 2)},
                SplitAttributes(("sk1", AttributeValueTypeCode.Int64), ("sk2", AttributeValueTypeCode.Int64))
            ) {ExpectedResult = "SplitFilter attribute keys must be in SplitAttributes: {sk1, sk2}"};

            yield return new TestCaseData(
                new[] {Attribute("sk2", value: 42), Attribute("SK2", value: true)},
                SplitAttributes(("sk1", AttributeValueTypeCode.Int64), ("sk2", AttributeValueTypeCode.Int64))
            ) {ExpectedResult = "SplitFilter attribute keys must be in SplitAttributes: {sk1, sk2}"};

            yield return new TestCaseData(
                new[] {Attribute(key: null!, value: 42)},
                SplitAttributes(("sk1", AttributeValueTypeCode.Int64))
            ) {ExpectedResult = "SplitFilter attribute keys must be in SplitAttributes: {sk1}"};

            yield return new TestCaseData(
                new[] {Attribute(key: string.Empty, value: 42)},
                SplitAttributes(("sk1", AttributeValueTypeCode.Int64))
            ) {ExpectedResult = "SplitFilter attribute keys must be in SplitAttributes: {sk1}"};

            yield return new TestCaseData(
                new[] {Attribute("sk1", value: 42), Attribute("sk1", value: 24)},
                SplitAttributes(("sk1", AttributeValueTypeCode.Int64), ("sk2", AttributeValueTypeCode.Int64))
            ) {ExpectedResult = "SplitFilter attribute keys must be unique"};

            yield return new TestCaseData(
                new[] {Attribute("sk1", value: true)},
                SplitAttributes(("sk1", AttributeValueTypeCode.Guid))
            ) {ExpectedResult = "SplitFilter attribute 'sk1' has invalid value: 'True'"};

            yield return new TestCaseData(
                new[] {Attribute("sk1", value: null)},
                SplitAttributes(("sk1", AttributeValueTypeCode.String))
            ) {ExpectedResult = "SplitFilter attribute 'sk1' has invalid value: ''"};

            yield return new TestCaseData(
                new[] {Attribute("sk1", value: 42)},
                SplitAttributes(("sk1", AttributeValueTypeCode.Int64))
            ) {ExpectedResult = string.Empty};

            yield return new TestCaseData(
                new[] {Attribute("sk2", value: true)},
                SplitAttributes(("sk1", AttributeValueTypeCode.Int64), ("sk2", AttributeValueTypeCode.Bool))
            ) {ExpectedResult = string.Empty};

            yield return new TestCaseData(
                new[] {Attribute("sk1", value: 42), Attribute("sk2", value: "la-la-la")},
                SplitAttributes(("sk1", AttributeValueTypeCode.Int64), ("sk2", AttributeValueTypeCode.String))
            ) {ExpectedResult = string.Empty};
        }

        private static (string Key, AttributeValueTypeCode Type)[] SplitAttributes(params (string Key, AttributeValueTypeCode Type)[] splitAttributes)
        {
            return splitAttributes;
        }
    }
}
