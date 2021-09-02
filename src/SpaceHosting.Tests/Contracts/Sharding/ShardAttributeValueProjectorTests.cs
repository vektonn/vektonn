using FakeItEasy;
using NUnit.Framework;
using SpaceHosting.Contracts;
using SpaceHosting.Contracts.Sharding;

namespace SpaceHosting.Tests.Contracts.Sharding
{
    public class ShardAttributeValueProjectorTests
    {
        private IAttributeValueHasher hasher = null!;

        [SetUp]
        public void SetUp()
        {
            hasher = A.Fake<IAttributeValueHasher>();
        }

        [TestCase((ushort)1, ExpectedResult = (ushort)0)]
        [TestCase((ushort)2, ExpectedResult = (ushort)0)]
        [TestCase((ushort)3, ExpectedResult = (ushort)0)]
        [TestCase((ushort)4, ExpectedResult = (ushort)2)]
        [TestCase((ushort)5, ExpectedResult = (ushort)2)]
        [TestCase((ushort)6, ExpectedResult = (ushort)0)]
        [TestCase((ushort)7, ExpectedResult = (ushort)0)]
        [TestCase((ushort)8, ExpectedResult = (ushort)2)]
        [TestCase((ushort)9, ExpectedResult = (ushort)6)]
        [TestCase((ushort)11, ExpectedResult = (ushort)9)]
        [TestCase((ushort)39, ExpectedResult = (ushort)3)]
        [TestCase((ushort)40, ExpectedResult = (ushort)2)]
        [TestCase((ushort)41, ExpectedResult = (ushort)1)]
        [TestCase((ushort)42, ExpectedResult = (ushort)0)]
        [TestCase((ushort)43, ExpectedResult = (ushort)42)]
        [TestCase((ushort)255, ExpectedResult = (ushort)42)]
        [TestCase((ushort)256, ExpectedResult = (ushort)42)]
        public ushort GetProjection(ushort numberOfShards)
        {
            var sut = new ShardAttributeValueProjector(numberOfShards, hasher);

            var attributeValue = new AttributeValue();
            A.CallTo(() => hasher.ComputeHash(attributeValue)).Returns(42UL);

            return sut.GetProjection(attributeValue);
        }
    }
}
