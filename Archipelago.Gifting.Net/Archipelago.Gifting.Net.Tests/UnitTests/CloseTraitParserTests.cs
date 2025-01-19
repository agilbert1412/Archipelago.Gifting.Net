using Archipelago.Gifting.Net.Gifts.Versions.Current;
using Archipelago.Gifting.Net.Traits;
using Archipelago.Gifting.Net.Utilities.CloseTraitParser;
using FluentAssertions;

namespace Archipelago.Gifting.Net.Tests.UnitTests
{
    public class CloseTraitParserTests
    {
        [Test]
        public void TestOneExactMatch()
        {
            ICloseTraitParser<int> closeTraitParser = new BKTreeCloseTraitParser<int>();
            closeTraitParser.RegisterAvailableGift(1, new[] { new GiftTrait("a", 1, 1) });
            closeTraitParser.RegisterAvailableGift(2, new[] { new GiftTrait("b", 1, 1) });
            closeTraitParser.RegisterAvailableGift(3, new[]
            {
                new GiftTrait("a", 1, 1),
                new GiftTrait("b", 1, 1)
            });
            var matches = closeTraitParser.FindClosestAvailableGift(new[] { new GiftTrait("a", 1, 1) });
            matches.Count.Should().Be(1);
            matches[0].Should().Be(1);
        }


        [Test]
        public void TestTwoExactMatches()
        {
            ICloseTraitParser<int> closeTraitParser = new BKTreeCloseTraitParser<int>();
            closeTraitParser.RegisterAvailableGift(1, new[] { new GiftTrait("a", 1, 1) });
            closeTraitParser.RegisterAvailableGift(2, new[] { new GiftTrait("a", 1, 1) });
            closeTraitParser.RegisterAvailableGift(3, new[] { new GiftTrait("b", 1, 1) });
            closeTraitParser.RegisterAvailableGift(4, new[]
            {
                new GiftTrait("a", 1, 1),
                new GiftTrait("b", 1, 1)
            });
            var matches = closeTraitParser.FindClosestAvailableGift(new[] { new GiftTrait("a", 1, 1) });
            matches.Count.Should().Be(2);
            matches.Should().Contain(1);
            matches.Should().Contain(2);
        }

        [Test]
        public void TestOneFuzzyMatch()
        {
            ICloseTraitParser<int> closeTraitParser = new BKTreeCloseTraitParser<int>();
            closeTraitParser.RegisterAvailableGift(1, new[] { new GiftTrait("a", 1, 1) });
            closeTraitParser.RegisterAvailableGift(2, new[] { new GiftTrait("b", 1, 1) });
            closeTraitParser.RegisterAvailableGift(3, new[]
            {
                new GiftTrait("a", 1, 1),
                new GiftTrait("b", 1, 1)
            });
            var matches = closeTraitParser.FindClosestAvailableGift(new[] { new GiftTrait("a", 2, 1) });
            matches.Count.Should().Be(1);
            matches[0].Should().Be(1);
        }

        [Test]
        public void TestTwoFuzzyMatches()
        {
            ICloseTraitParser<int> closeTraitParser = new BKTreeCloseTraitParser<int>();
            closeTraitParser.RegisterAvailableGift(1, new[]
            {
                new GiftTrait("a", 1, 1),
                new GiftTrait("b", 1, 1)
            });
            closeTraitParser.RegisterAvailableGift(2, new[]
            {
                new GiftTrait("a", 1, 1),
                new GiftTrait("c", 1, 1)
            });
            closeTraitParser.RegisterAvailableGift(3, new[]
            {
                new GiftTrait("d", 1, 1)
            });
            var matches = closeTraitParser.FindClosestAvailableGift(new[] { new GiftTrait("a", 1, 1) });
            matches.Count.Should().Be(2);
            matches.Should().Contain(1);
            matches.Should().Contain(2);
        }

        [Test]
        public void TestNoMatch()
        {
            ICloseTraitParser<int> closeTraitParser = new BKTreeCloseTraitParser<int>();
            closeTraitParser.RegisterAvailableGift(1, new[]
            {
                new GiftTrait("a", 1, 1)
            });
            var matches = closeTraitParser.FindClosestAvailableGift(new[] { new GiftTrait("b", 1, 1) });
            matches.Count.Should().Be(0);
        }

        [Test]
        public void TestGoodClosest()
        {
            ICloseTraitParser<int> closeTraitParser = new BKTreeCloseTraitParser<int>();
            closeTraitParser.RegisterAvailableGift(1, new[]
            {
                new GiftTrait("a", 1, 1)
            });
            closeTraitParser.RegisterAvailableGift(2, new[]
            {
                new GiftTrait("a", 1, 1),
                new GiftTrait("b", 20, 1)
            });
            var matches = closeTraitParser.FindClosestAvailableGift(new[]
            {
                new GiftTrait("a", 1, 1),
                new GiftTrait("b", 1, 1)
            });
            matches.Count.Should().Be(1);
            matches[0].Should().Be(2);
        }
    }
}
