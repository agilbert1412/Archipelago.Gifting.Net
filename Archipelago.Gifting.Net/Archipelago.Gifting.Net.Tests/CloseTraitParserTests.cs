using Archipelago.Gifting.Net.Gifts;
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
            closeTraitParser.RegisterGiftItem(new GiftItem("1", 1, 0), new[] { new GiftTrait("a", 1, 1) });
            closeTraitParser.RegisterGiftItem(new GiftItem("2", 1, 0), new[] { new GiftTrait("b", 1, 1) });
            closeTraitParser.RegisterGiftItem(new GiftItem("3", 1, 0), new[]
            {
                new GiftTrait("a", 1, 1),
                new GiftTrait("b", 1, 1)
            });
            List<GiftItem>? matches = closeTraitParser.FindClosest(new[] { new GiftTrait("a", 1, 1) });
            matches.Count.Should().Be(1);
            matches[0].Name.Should().Be("1");
        }


        [Test]
        public void TestTwoExactMatches()
        {
            ICloseTraitParser closeTraitParser = new BKTreeCloseTraitParser();
            closeTraitParser.RegisterGiftItem(new GiftItem("1", 1, 0), new[] { new GiftTrait("a", 1, 1) });
            closeTraitParser.RegisterGiftItem(new GiftItem("2", 1, 0), new[] { new GiftTrait("a", 1, 1) });
            closeTraitParser.RegisterGiftItem(new GiftItem("3", 1, 0), new[] { new GiftTrait("b", 1, 1) });
            closeTraitParser.RegisterGiftItem(new GiftItem("4", 1, 0), new[]
            {
                new GiftTrait("a", 1, 1),
                new GiftTrait("b", 1, 1)
            });
            List<GiftItem>? matches = closeTraitParser.FindClosest(new[] { new GiftTrait("a", 1, 1) });
            matches.Count.Should().Be(2);
            matches.Should().Contain(item => item.Name == "1");
            matches.Should().Contain(item => item.Name == "2");
        }

        [Test]
        public void TestOneFuzzyMatch()
        {
            ICloseTraitParser closeTraitParser = new BKTreeCloseTraitParser();
            closeTraitParser.RegisterGiftItem(new GiftItem("1", 1, 0), new[] { new GiftTrait("a", 1, 1) });
            closeTraitParser.RegisterGiftItem(new GiftItem("2", 1, 0), new[] { new GiftTrait("b", 1, 1) });
            closeTraitParser.RegisterGiftItem(new GiftItem("3", 1, 0), new[]
            {
                new GiftTrait("a", 1, 1),
                new GiftTrait("b", 1, 1)
            });
            List<GiftItem>? matches = closeTraitParser.FindClosest(new[] { new GiftTrait("a", 2, 1) });
            matches.Count.Should().Be(1);
            matches[0].Name.Should().Be("1");
        }

        [Test]
        public void TestTwoFuzzyMatches()
        {
            ICloseTraitParser closeTraitParser = new BKTreeCloseTraitParser();
            closeTraitParser.RegisterGiftItem(new GiftItem("1", 1, 0), new[]
            {
                new GiftTrait("a", 1, 1),
                new GiftTrait("b", 1, 1)
            });
            closeTraitParser.RegisterGiftItem(new GiftItem("2", 1, 0), new[]
            {
                new GiftTrait("a", 1, 1),
                new GiftTrait("c", 1, 1)
            });
            List<GiftItem>? matches = closeTraitParser.FindClosest(new[] { new GiftTrait("a", 1, 1) });
            matches.Count.Should().Be(2);
            matches.Should().Contain(item => item.Name == "1");
            matches.Should().Contain(item => item.Name == "2");
        }
    }
}
