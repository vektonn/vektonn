using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Vektonn.Contracts;
using Vektonn.Contracts.ApiModels;
using Vektonn.Contracts.ApiModels.Validation;
using Vektonn.Contracts.Sharding.DataSource;
using Vektonn.Contracts.Sharding.Index;
using Vektonn.Index;
using static Vektonn.Tests.Contracts.ApiModels.AttributeDtoTestHelpers;
using static Vektonn.Tests.Contracts.ApiModels.VectorDtoTestHelpers;

namespace Vektonn.Tests.Contracts.ApiModels
{
    public class SearchQueryValidatorTests
    {
        [TestCaseSource(nameof(TestCases))]
        public string Validate(SearchQueryDto searchQuery, IndexMeta indexMeta)
        {
            var sut = new SearchQueryValidator(indexMeta);

            return sut.Validate(searchQuery).ToString(separator: " \n ");
        }

        private static IEnumerable<TestCaseData> TestCases()
        {
            yield return new TestCaseData(
                new SearchQueryDto(SplitFilter: null, DenseQueryVectors(), K: 0),
                IndexMetaDense()
            ) {ExpectedResult = "K must be positive"};

            yield return new TestCaseData(
                new SearchQueryDto(SplitFilter: null, QueryVectors: Array.Empty<VectorDto>(), K: 1),
                IndexMetaDense()
            ) {ExpectedResult = "At least one query vector is required"};

            yield return new TestCaseData(
                new SearchQueryDto(SplitFilter: null, DenseQueryVectors(), K: 1),
                IndexMetaSparse()
            ) {ExpectedResult = "Vector must be sparse"};

            yield return new TestCaseData(
                new SearchQueryDto(SplitFilter: null, SparseQueryVectors(), K: 1),
                IndexMetaDense()
            ) {ExpectedResult = "Vector must be dense"};

            yield return new TestCaseData(
                new SearchQueryDto(
                    SplitFilter: new[] {Attribute("sk0", value: 0)},
                    DenseQueryVectors(),
                    K: -1),
                IndexMetaDense(splitAttributes: new[] {("sk1", AttributeValueTypeCode.Int64)})
            ) {ExpectedResult = "K must be positive \n SplitFilter attribute keys must be in SplitAttributes: {sk1}"};

            yield return new TestCaseData(
                new SearchQueryDto(SplitFilter: null, SparseQueryVectors(), K: 1),
                IndexMetaSparse()
            ) {ExpectedResult = string.Empty};

            yield return new TestCaseData(
                new SearchQueryDto(SplitFilter: null, QueryVectors: DenseQueryVectors(), K: 7),
                IndexMetaDense()
            ) {ExpectedResult = string.Empty};

            yield return new TestCaseData(
                new SearchQueryDto(
                    SplitFilter: new[] {Attribute("sk1", value: 0), Attribute("sk2", value: true)},
                    QueryVectors: DenseQueryVectors(),
                    K: 1),
                IndexMetaDense(splitAttributes: new[] {("sk1", AttributeValueTypeCode.Int64), ("sk2", AttributeValueTypeCode.Bool)})
            ) {ExpectedResult = string.Empty};
        }

        private static IndexMeta IndexMetaDense((string Key, AttributeValueTypeCode Type)[]? splitAttributes = null)
        {
            var attributeValueTypes = (splitAttributes ?? Array.Empty<(string Key, AttributeValueTypeCode Type)>()).ToDictionary(t => t.Key, t => t.Type);
            return new IndexMeta(
                DataSourceMeta(vectorsAreSparse: false, attributeValueTypes),
                Algorithms.FaissIndexFlatIP,
                IdAttributes: new HashSet<string>(),
                SplitAttributes: attributeValueTypes.Keys.ToHashSet(),
                IndexShardsMap: new IndexShardsMapMeta(new Dictionary<string, IndexShardMeta>()));
        }

        private static IndexMeta IndexMetaSparse()
        {
            return new IndexMeta(
                DataSourceMeta(
                    vectorsAreSparse: true,
                    attributeValueTypes: new Dictionary<string, AttributeValueTypeCode>()),
                Algorithms.SparnnIndexCosine,
                IdAttributes: new HashSet<string>(),
                SplitAttributes: new HashSet<string>(),
                IndexShardsMap: new IndexShardsMapMeta(new Dictionary<string, IndexShardMeta>()));
        }

        private static DataSourceMeta DataSourceMeta(bool vectorsAreSparse, Dictionary<string, AttributeValueTypeCode> attributeValueTypes)
        {
            return new DataSourceMeta(
                TestVectorDimension,
                vectorsAreSparse,
                PermanentAttributes: new HashSet<string>(),
                DataSourceShardingMeta: new DataSourceShardingMeta(new Dictionary<string, IDataSourceAttributeValueSharder>()),
                attributeValueTypes
            );
        }
    }
}
