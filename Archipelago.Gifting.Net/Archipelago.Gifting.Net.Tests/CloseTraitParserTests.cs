using Archipelago.Gifting.Net.Traits;
using Archipelago.Gifting.Net.Utilities.CloseTraitParser;
using FluentAssertions;

namespace Archipelago.Gifting.Net.Tests
{
    public class CloseTraitParserTests
    {
        [Test]
        public void TestOneExactMatch()
        {
            ICloseTraitParser closeTraitParser = new BKTreeCloseTraitParser();
            closeTraitParser.RegisterAvailableGift(1, new[] { new GiftTrait("a", 1, 1) });
            closeTraitParser.RegisterAvailableGift(2, new[] { new GiftTrait("b", 1, 1) });
            closeTraitParser.RegisterAvailableGift(3, new[]
            {
                new GiftTrait("a", 1, 1),
                new GiftTrait("b", 1, 1)
            });
            List<object> matches = closeTraitParser.FindClosestAvailableGift(new[] { new GiftTrait("a", 1, 1) });
            matches.Count.Should().Be(1);
            matches[0].Should().Be(1);
        }


        [Test]
        public void TestTwoExactMatches()
        {
            ICloseTraitParser closeTraitParser = new BKTreeCloseTraitParser();
            closeTraitParser.RegisterAvailableGift(1, new[] { new GiftTrait("a", 1, 1) });
            closeTraitParser.RegisterAvailableGift(2, new[] { new GiftTrait("a", 1, 1) });
            closeTraitParser.RegisterAvailableGift(3, new[] { new GiftTrait("b", 1, 1) });
            closeTraitParser.RegisterAvailableGift(4, new[]
            {
                new GiftTrait("a", 1, 1),
                new GiftTrait("b", 1, 1)
            });
            List<object> matches = closeTraitParser.FindClosestAvailableGift(new[] { new GiftTrait("a", 1, 1) });
            matches.Count.Should().Be(2);
            matches.Should().Contain(1);
            matches.Should().Contain(2);
        }

        [Test]
        public void TestOneFuzzyMatch()
        {
            ICloseTraitParser closeTraitParser = new BKTreeCloseTraitParser();
            closeTraitParser.RegisterAvailableGift(1, new[] { new GiftTrait("a", 1, 1) });
            closeTraitParser.RegisterAvailableGift(2, new[] { new GiftTrait("b", 1, 1) });
            closeTraitParser.RegisterAvailableGift(3, new[]
            {
                new GiftTrait("a", 1, 1),
                new GiftTrait("b", 1, 1)
            });
            List<object> matches = closeTraitParser.FindClosestAvailableGift(new[] { new GiftTrait("a", 2, 1) });
            matches.Count.Should().Be(1);
            matches[0].Should().Be(1);
        }

        [Test]
        public void TestTwoFuzzyMatches()
        {
            ICloseTraitParser closeTraitParser = new BKTreeCloseTraitParser();
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
            List<object> matches = closeTraitParser.FindClosestAvailableGift(new[] { new GiftTrait("a", 1, 1) });
            matches.Count.Should().Be(2);
            matches.Should().Contain(1);
            matches.Should().Contain(2);
        }
    }
}
