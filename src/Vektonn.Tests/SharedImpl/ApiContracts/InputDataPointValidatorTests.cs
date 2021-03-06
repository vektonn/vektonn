using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Vektonn.ApiContracts;
using Vektonn.SharedImpl.ApiContracts.Validation;
using Vektonn.SharedImpl.Contracts;
using Vektonn.SharedImpl.Contracts.Sharding;
using Vektonn.SharedImpl.Contracts.Sharding.DataSource;
using static Vektonn.Tests.SharedImpl.ApiContracts.AttributeDtoTestHelpers;
using static Vektonn.Tests.SharedImpl.ApiContracts.VectorDtoTestHelpers;

namespace Vektonn.Tests.SharedImpl.ApiContracts
{
    public class InputDataPointValidatorTests
    {
        [TestCaseSource(nameof(TestCases))]
        public string Validate(InputDataPointDto inputDataPoint, DataSourceMeta dataSourceMeta)
        {
            var sut = new InputDataPointValidator(dataSourceMeta);

            return sut.Validate(inputDataPoint).ToString(separator: " \n ");
        }

        private static IEnumerable<TestCaseData> TestCases()
        {
            yield return new TestCaseData(
                new InputDataPointDto(new[] {Attribute("key", value: 42), Attribute("key", value: 24)}, DenseVector(), IsDeleted: false),
                DataSourceMeta(vectorsAreSparse: false, KnownAttributes(("key", AttributeValueTypeCode.Int64)))
            ) {ExpectedResult = "Attribute keys must be unique"};

            yield return new TestCaseData(
                new InputDataPointDto(new[] {Attribute("key", value: 42), Attribute("extraKey", value: 24)}, DenseVector(), IsDeleted: false),
                DataSourceMeta(vectorsAreSparse: false, KnownAttributes(("key", AttributeValueTypeCode.Int64)))
            ) {ExpectedResult = "All attributes defined for data source (and no other ones) must be specified \n InputDataPoint attribute key is unknown: 'extraKey'"};

            yield return new TestCaseData(
                new InputDataPointDto(new[] {Attribute("key", value: 42)}, DenseVector(), IsDeleted: false),
                DataSourceMeta(vectorsAreSparse: false, KnownAttributes(("key", AttributeValueTypeCode.Int64), ("extraKey", AttributeValueTypeCode.Int64)))
            ) {ExpectedResult = "All attributes defined for data source (and no other ones) must be specified"};

            yield return new TestCaseData(
                new InputDataPointDto(new[] {Attribute("k1", value: 42), Attribute("k2", value: 42)}, DenseVector(), IsDeleted: false),
                DataSourceMeta(vectorsAreSparse: false, KnownAttributes(("k2", AttributeValueTypeCode.Int64), ("k3", AttributeValueTypeCode.Int64)))
            ) {ExpectedResult = "All attributes defined for data source (and no other ones) must be specified \n InputDataPoint attribute key is unknown: 'k1'"};

            yield return new TestCaseData(
                new InputDataPointDto(new[] {Attribute("key", value: 42)}, DenseVector(), IsDeleted: false),
                DataSourceMeta(vectorsAreSparse: false, KnownAttributes(("key", AttributeValueTypeCode.String)))
            ) {ExpectedResult = "InputDataPoint attribute 'key' has invalid value: '42'"};

            yield return new TestCaseData(
                new InputDataPointDto(new[] {Attribute("key", value: 42)}, Vector: null, IsDeleted: false),
                DataSourceMeta(vectorsAreSparse: false, KnownAttributes(("key", AttributeValueTypeCode.Int64)))
            ) {ExpectedResult = "IsDeleted is inconsistent with Vector"};

            yield return new TestCaseData(
                new InputDataPointDto(new[] {Attribute("key", value: 42)}, DenseVector(), IsDeleted: true),
                DataSourceMeta(vectorsAreSparse: false, KnownAttributes(("key", AttributeValueTypeCode.Int64)))
            ) {ExpectedResult = "IsDeleted is inconsistent with Vector"};

            yield return new TestCaseData(
                new InputDataPointDto(new[] {Attribute("key", value: 42)}, DenseVector(), IsDeleted: false),
                DataSourceMeta(vectorsAreSparse: true, KnownAttributes(("key", AttributeValueTypeCode.Int64)))
            ) {ExpectedResult = "Vector must be sparse"};

            yield return new TestCaseData(
                new InputDataPointDto(new[] {Attribute("key", value: 42)}, SparseVector(), IsDeleted: false),
                DataSourceMeta(vectorsAreSparse: false, KnownAttributes(("key", AttributeValueTypeCode.Int64)))
            ) {ExpectedResult = "Vector must be dense"};

            yield return new TestCaseData(
                new InputDataPointDto(new[] {Attribute("KEY", value: 42)}, DenseVector(), IsDeleted: true),
                DataSourceMeta(vectorsAreSparse: false, KnownAttributes(("key", AttributeValueTypeCode.Int64)))
            ) {ExpectedResult = "All attributes defined for data source (and no other ones) must be specified \n InputDataPoint attribute key is unknown: 'KEY' \n IsDeleted is inconsistent with Vector"};

            yield return new TestCaseData(
                new InputDataPointDto(new[] {Attribute("k1", value: true), Attribute("k2", value: "la-la-la")}, DenseVector(), IsDeleted: false),
                DataSourceMeta(
                    vectorsAreSparse: false,
                    KnownAttributes(("k1", AttributeValueTypeCode.Bool), ("k2", AttributeValueTypeCode.String)),
                    shardingMeta: new[] {("k2", PossibleValues: new[] {"bu-bu-bu"})}
                )
            ) {ExpectedResult = "InputDataPoint attribute 'k2' has unacceptable value 'la-la-la' for corresponding sharder"};

            yield return new TestCaseData(
                new InputDataPointDto(new[] {Attribute("k1", value: true), Attribute("k2", value: "la-la-la")}, DenseVector(), IsDeleted: false),
                DataSourceMeta(
                    vectorsAreSparse: false,
                    KnownAttributes(("k1", AttributeValueTypeCode.Bool), ("k2", AttributeValueTypeCode.String)),
                    shardingMeta: new[] {("k2", PossibleValues: new[] {"la-la-la", "bu-bu-bu"})}
                )
            ) {ExpectedResult = string.Empty};

            yield return new TestCaseData(
                new InputDataPointDto(new[] {Attribute("k1", value: true), Attribute("k2", value: "la-la-la")}, DenseVector(), IsDeleted: false),
                DataSourceMeta(vectorsAreSparse: false, KnownAttributes(("k1", AttributeValueTypeCode.Bool), ("k2", AttributeValueTypeCode.String)))
            ) {ExpectedResult = string.Empty};

            yield return new TestCaseData(
                new InputDataPointDto(new[] {Attribute("key", value: 42)}, SparseVector(), IsDeleted: false),
                DataSourceMeta(vectorsAreSparse: true, KnownAttributes(("key", AttributeValueTypeCode.Int64)))
            ) {ExpectedResult = string.Empty};

            yield return new TestCaseData(
                new InputDataPointDto(new[] {Attribute("key", value: 42)}, Vector: null, IsDeleted: true),
                DataSourceMeta(vectorsAreSparse: true, KnownAttributes(("key", AttributeValueTypeCode.Int64)))
            ) {ExpectedResult = string.Empty};

            yield return new TestCaseData(
                new InputDataPointDto(Array.Empty<AttributeDto>(), SparseVector(), IsDeleted: false),
                DataSourceMeta(vectorsAreSparse: true, KnownAttributes())
            ) {ExpectedResult = string.Empty};
        }

        private static DataSourceMeta DataSourceMeta(
            bool vectorsAreSparse,
            (string Key, AttributeValueTypeCode Type)[] knownAttributes,
            (string Key, string[] PossibleValues)[]? shardingMeta = null)
        {
            var attributeValueSharders = (shardingMeta ?? Array.Empty<(string Key, string[] PossibleValues)>()).ToDictionary(
                t => t.Key,
                t => (IDataSourceAttributeValueSharder)new ValueBasedDataSourceAttributeValueSharder(
                    new AttributeValueHasher(),
                    possibleValues: t.PossibleValues.Select(x => new AttributeValue(String: x)).ToHashSet())
            );

            return new DataSourceMeta(
                new DataSourceId(Name: "test", Version: "1.0"),
                TestVectorDimension,
                vectorsAreSparse,
                PermanentAttributes: attributeValueSharders.Keys.ToHashSet(),
                DataSourceShardingMeta: new DataSourceShardingMeta(attributeValueSharders),
                AttributeValueTypes: knownAttributes.ToDictionary(t => t.Key, t => t.Type)
            );
        }
    }
}
