using Archipelago.Gifting.Net.Traits;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using FluentAssertions;

namespace Archipelago.Gifting.Net.Tests.IntegrationTests
{
    public class OutdatedGiftBoxesIntegrationTests : IntegrationTestBase
    {
        [Test]
        public void TestCanReadOutdatedGiftBoxVersion2()
        {
            // Arrange
            _serviceReceiver.CloseGiftBox();
            WaitShort();

            var motherboxKey = $"GiftBoxes;{_testSessions.ReceiverTeam}";
            var myGiftBoxEntry = new Dictionary<int, Versioning.GiftBoxes.Version2.GiftBox>
            {
                { _testSessions.ReceiverSlot, new Versioning.GiftBoxes.Version2.GiftBox(false, new[] { GiftFlag.Food, GiftFlag.Drink }) },
            };
            _sessionReceiver.DataStorage[Scope.Global, motherboxKey] += Operation.Update(myGiftBoxEntry);
            WaitShort();

            // Act
            var canGiftAnything = _serviceSender.CanGiftToPlayer(_testSessions.ReceiverSlot);
            var canGiftFood = _serviceSender.CanGiftToPlayer(_testSessions.ReceiverSlot, new[] { GiftFlag.Food });
            var canGiftFoodAndDamage = _serviceSender.CanGiftToPlayer(_testSessions.ReceiverSlot, new[] { GiftFlag.Food, GiftFlag.Damage });
            var canGiftDamageAndEgg = _serviceSender.CanGiftToPlayer(_testSessions.ReceiverSlot, new[] { GiftFlag.Damage, GiftFlag.Egg });

            // Assert
            canGiftAnything.Should().BeFalse();
            canGiftFood.Should().BeTrue();
            canGiftFoodAndDamage.Should().BeTrue();
            canGiftDamageAndEgg.Should().BeFalse();
        }
    }
}