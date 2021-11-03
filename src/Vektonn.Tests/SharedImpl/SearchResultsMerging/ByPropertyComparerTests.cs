using FluentAssertions;
using NUnit.Framework;
using Vektonn.SharedImpl.SearchResultsMerging;

namespace Vektonn.Tests.SharedImpl.SearchResultsMerging
{
    public class ByPropertyComparerTests
    {
        private readonly ByPropertyComparer<Item, string> sut = new(item => item.Prop1);

        [Test]
        public void Test()
        {
            var item1 = new Item {Prop1 = "a"};
            var item2 = new Item {Prop1 = "b"};
            var item3 = new Item {Prop1 = "a"};

            sut.Compare(item1, item2).Should().Be(-1);
            sut.Compare(item1, item3).Should().Be(0);
            sut.Compare(item2, item1).Should().Be(1);
        }

        [Test]
        public void Test_WithNull()
        {
            var item = new Item {Prop1 = "x"};

            sut.Compare(null, item).Should().Be(-1);
            sut.Compare(null, null).Should().Be(0);
            sut.Compare(item, null).Should().Be(1);
        }

        private class Item
        {
            public string Prop1 { get; init; } = default!;
        }
    }
}
