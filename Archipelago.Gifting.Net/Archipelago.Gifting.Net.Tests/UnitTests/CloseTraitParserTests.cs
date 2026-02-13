using Archipelago.Gifting.Net.Traits;
using Archipelago.Gifting.Net.Utilities.CloseTraitParser;
using Archipelago.Gifting.Net.Versioning.Gifts.Current;
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

        [Test]
        public void TestTriangularInequalityWithBaseDistance()
        {
            BKTreeCloseTraitParser<int> closeTraitParser = new();
            Random random = new();
            
            double RandomWeight()
            {
                switch (random.Next(0, 5))
                {
                    case <2:
                        return random.NextDouble();
                    case 3:
                        return 0;
                    default:
                        return random.NextDouble() * 9 + 1;
                }
            }
            
            for (int i = 0; i < 20; i++)
            {
                closeTraitParser.RegisterAvailableGift(i, new[]
                {
                    new GiftTrait("a", RandomWeight(), RandomWeight())
                });
            }
            for (int i = 20; i < 40; i++)
            {
                closeTraitParser.RegisterAvailableGift(i, new[]
                {
                    new GiftTrait("b", RandomWeight(), RandomWeight())
                });
            }
            for (int i = 40; i < 60; i++)
            {
                closeTraitParser.RegisterAvailableGift(i, new[]
                {
                    new GiftTrait("c", RandomWeight(), RandomWeight())
                });
            }
            for (int i = 60; i < 80; i++)
            {
                closeTraitParser.RegisterAvailableGift(i, new[]
                {
                    new GiftTrait("a", RandomWeight(), RandomWeight()),
                    new GiftTrait("b", RandomWeight(), RandomWeight())
                });
            }
            for (int i = 80; i < 100; i++)
            {
                closeTraitParser.RegisterAvailableGift(i, new[]
                {
                    new GiftTrait("b", RandomWeight(), RandomWeight()),
                    new GiftTrait("c", RandomWeight(), RandomWeight())
                });
            }
            for (int i = 100; i < 120; i++)
            {
                closeTraitParser.RegisterAvailableGift(i, new[]
                {
                    new GiftTrait("a", RandomWeight(), RandomWeight()),
                    new GiftTrait("c", RandomWeight(), RandomWeight())
                });
            }
            for (int i = 120; i < 140; i++)
            {
                closeTraitParser.RegisterAvailableGift(i, new[]
                {
                    new GiftTrait("a", RandomWeight(), RandomWeight()),
                    new GiftTrait("b", RandomWeight(), RandomWeight()),
                    new GiftTrait("c", RandomWeight(), RandomWeight())
                });
            }
            closeTraitParser.CheckConsistency();
        }
    }
}
