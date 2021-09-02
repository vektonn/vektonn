using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SpaceHosting.IndexShard.Models;
using SpaceHosting.IndexShard.Models.Sharding;

namespace SpaceHosting.IndexShard.Tests.Sharding
{
    public class AttributeValueHasherTests
    {
        private readonly AttributeValueHasher sut = new();

        [Test]
        public void ComputeHash_EmptyAttributeValue()
        {
            Action action = () => sut.ComputeHash(new AttributeValue());
            action.Should().Throw<InvalidOperationException>().WithMessage("Invalid AttributeValue: ");
        }

        [TestCaseSource(nameof(TestCases))]
        public ulong ComputeHash(AttributeValue attributeValue)
        {
            return sut.ComputeHash(attributeValue);
        }

        private static IEnumerable<TestCaseData> TestCases()
        {
            yield return new TestCaseData(new AttributeValue(Bool: false)) {ExpectedResult = 0UL};
            yield return new TestCaseData(new AttributeValue(Bool: true)) {ExpectedResult = 1UL};

            yield return new TestCaseData(new AttributeValue(Guid: Guid.Empty)) {ExpectedResult = 3499346722691045397UL};
            yield return new TestCaseData(new AttributeValue(Guid: Guid.Parse("e8edb009-dd41-43a5-8d64-63458ebd0341"))) {ExpectedResult = 8057476853770941118UL};

            yield return new TestCaseData(new AttributeValue(String: string.Empty)) {TestName = "{m}([EmptyString])", ExpectedResult = 11160318154034397263UL};
            yield return new TestCaseData(new AttributeValue(String: "x")) {ExpectedResult = 2238803048373571102UL};
            yield return new TestCaseData(new AttributeValue(String: "xy")) {ExpectedResult = 13837061459715779799UL};
            yield return new TestCaseData(new AttributeValue(String: "xyz")) {ExpectedResult = 1989124643368747521UL};
            yield return new TestCaseData(new AttributeValue(String: "e8edb009-dd41-43a5-8d64-63458ebd0341")) {ExpectedResult = 6654175706423802152UL};
            yield return new TestCaseData(new AttributeValue(String: "Русские буквы")) {ExpectedResult = 13999472007608230140UL};
            yield return new TestCaseData(new AttributeValue(String: "Some unicode symbols: A ⋂ Ā ≡ ∅")) {ExpectedResult = 16939182229021361002UL};

            yield return new TestCaseData(new AttributeValue(Int64: 0L)) {ExpectedResult = 1457330246272086660UL};
            yield return new TestCaseData(new AttributeValue(Int64: 1L)) {ExpectedResult = 5925585971146611297UL};
            yield return new TestCaseData(new AttributeValue(Int64: -1L)) {ExpectedResult = 3458737730936475989UL};
            yield return new TestCaseData(new AttributeValue(Int64: 42L)) {ExpectedResult = 15591584478111741110UL};
            yield return new TestCaseData(new AttributeValue(Int64: byte.MaxValue)) {ExpectedResult = 8843577790303579752UL};
            yield return new TestCaseData(new AttributeValue(Int64: short.MaxValue)) {ExpectedResult = 13545287680376294408UL};
            yield return new TestCaseData(new AttributeValue(Int64: int.MaxValue)) {ExpectedResult = 12509318086319848728UL};
            yield return new TestCaseData(new AttributeValue(Int64: long.MaxValue)) {ExpectedResult = 16359246543468789202UL};
            yield return new TestCaseData(new AttributeValue(Int64: long.MinValue)) {ExpectedResult = 7578918962206252314UL};

            yield return new TestCaseData(new AttributeValue(DateTime: new DateTime(2021, 07, 31, 13, 52, 59, DateTimeKind.Utc))) {ExpectedResult = 11262061747148714790UL};
        }
    }
}
