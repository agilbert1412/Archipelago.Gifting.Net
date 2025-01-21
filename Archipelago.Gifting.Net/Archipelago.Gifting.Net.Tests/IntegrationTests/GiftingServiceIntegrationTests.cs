using Archipelago.Gifting.Net.Gifts;
using Archipelago.Gifting.Net.Gifts.Versions.Current;
using Archipelago.Gifting.Net.Traits;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Archipelago.Gifting.Net.Tests.IntegrationTests
{
    public class GiftingServiceIntegrationTests : IntegrationTestBase
    {
        [Test]
        public void TestCannotSendGiftToNeverOpenedBox()
        {
            // Arrange
            CloseReceiverGiftBox();
            Wait();

            // Assume

            // Act
            var canGift = _serviceSender.CanGiftToPlayer(ReceiverName);

            // Assert
            canGift.Should().BeFalse();
        }

        [Test]
        public void TestOpenGiftboxRegistersAsCanSendGift()
        {
            // Arrange

            // Assume
            var canGift = _serviceSender.CanGiftToPlayer(ReceiverName);
            canGift.Should().BeFalse();

            // Act
            _serviceReceiver.OpenGiftBox();
            Wait();

            // Assert
            canGift = _serviceSender.CanGiftToPlayer(ReceiverName);
            canGift.Should().BeTrue();
        }

        [Test]
        public void TestCannotSendGiftToNonExistingPlayer()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            Wait();

            // Assume

            // Act
            var canGift = _serviceSender.CanGiftToPlayer("Fake Player");

            // Assert
            canGift.Should().BeFalse();
        }

        [Test]
        public void TestCanSendGiftToAlias()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            Wait();
            SetAlias(_sessionReceiver, "receiver2");

            // Assume

            // Act
            var canGift = _serviceSender.CanGiftToPlayer("receiver2");

            // Assert
            canGift.Should().BeTrue();
        }

        [Test]
        public void TestCannotSendGiftToDuplicateAlias()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            Wait();
            SetAlias(_sessionSender, "receiver2");
            SetAlias(_sessionReceiver, "receiver2");

            // Assume

            // Act
            var canGift = _serviceSender.CanGiftToPlayer("receiver2");

            // Assert
            canGift.Should().BeFalse();
        }

        [Test]
        public void TestCanSendGiftToNameEvenIfAliasExists()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            Wait();
            SetAlias(_sessionSender, ReceiverName);
            SetAlias(_sessionReceiver, SenderName);

            // Assume

            // Act
            var canGift = _serviceSender.CanGiftToPlayer(ReceiverName);

            // Assert
            canGift.Should().BeTrue();
        }

        [Test]
        public void TestOpenGiftboxCreatesEmptyGiftDictionary()
        {
            // Arrange

            // Assume
            var giftsBeforeOpeningBox = _serviceSender.CheckGiftBox();
            giftsBeforeOpeningBox.Should().BeEmpty();

            // Act
            _serviceSender.OpenGiftBox();

            // Assert
            var giftsAfterOpeningBox = _serviceSender.CheckGiftBox();
            giftsAfterOpeningBox.Should().BeEmpty();
        }

        [Test]
        public void TestCloseGiftboxShouldCannotSendGiftsAnymore()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            Wait();

            // Assume
            var canGift = _serviceSender.CanGiftToPlayer(ReceiverName);
            canGift.Should().BeTrue();

            // Act
            _serviceReceiver.CloseGiftBox();
            Wait();

            // Assert
            canGift = _serviceSender.CanGiftToPlayer(ReceiverName);
            canGift.Should().BeFalse();
        }

        [Test]
        public void TestCloseGiftboxShouldNotTurnGiftsToNull()
        {
            // Arrange
            _serviceSender.OpenGiftBox();
            Wait();

            // Assume
            var giftsBeforeClosingBox = _serviceSender.CheckGiftBox();
            giftsBeforeClosingBox.Should().NotBeNull().And.BeEmpty();

            // Act
            _serviceSender.CloseGiftBox();

            // Assert
            var giftsAfterClosingBox = _serviceSender.CheckGiftBox();
            giftsBeforeClosingBox.Should().NotBeNull().And.BeEmpty();
        }

        [Test]
        public void TestSendGiftToClosedBoxFails()
        {
            // Arrange
            _serviceReceiver.CloseGiftBox();
            var gift = NewGiftItem();
            Wait();

            // Assume
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().BeEmpty();

            // Act
            var result = _serviceSender.SendGift(gift, ReceiverName);

            // Assert
            result.Success.Should().BeFalse();
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().BeEmpty();
        }

        [Test]
        public void TestSendGiftToOpenBoxSucceeds()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            var gift = NewGiftItem();
            Wait();

            // Assume
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().BeEmpty();

            // Act
            var result = _serviceSender.SendGift(gift, ReceiverName);
            Wait();

            // Assert
            result.Success.Should().BeTrue();
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(1);
            var (receivedGiftId, receivedGift) = gifts.First();
            receivedGiftId.Should().Be(result.GiftId);
            receivedGift.id.Should().Be(result.GiftId);
            receivedGift.itemName.Should().Be(gift.Name);
            receivedGift.amount.Should().Be(gift.Amount);
            receivedGift.itemValue.Should().Be(gift.Value);
            receivedGift.senderSlot.Should().Be(_testSessions.SenderSlot);
            receivedGift.receiverSlot.Should().Be(_testSessions.ReceiverSlot);
            receivedGift.senderTeam.Should().Be(receivedGift.receiverTeam);
        }

        [Test]
        public void TestSendTwoGiftToOpenBoxStacksGifts()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            var gift1 = NewGiftItem();
            var gift2 = NewGiftItem();
            Wait();

            // Assume
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().BeEmpty();

            // Act
            var result1 = _serviceSender.SendGift(gift1, ReceiverName);
            var result2 = _serviceSender.SendGift(gift2, ReceiverName);
            Wait();

            // Assert
            result1.Success.Should().BeTrue();
            result2.Success.Should().BeTrue();
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(2);
            var receivedGift1 = gifts[result1.GiftId];
            var receivedGift2 = gifts[result2.GiftId];
            receivedGift1.itemName.Should().Be(gift1.Name);
            receivedGift1.amount.Should().Be(gift1.Amount);
            receivedGift1.itemValue.Should().Be(gift1.Value);
            receivedGift1.senderSlot.Should().Be(_testSessions.SenderSlot);
            receivedGift1.receiverSlot.Should().Be(_testSessions.ReceiverSlot);
            receivedGift1.senderTeam.Should().Be(receivedGift1.receiverTeam);
            receivedGift2.itemName.Should().Be(gift2.Name);
            receivedGift2.amount.Should().Be(gift2.Amount);
            receivedGift2.itemValue.Should().Be(gift2.Value);
            receivedGift2.senderSlot.Should().Be(_testSessions.SenderSlot);
            receivedGift2.receiverSlot.Should().Be(_testSessions.ReceiverSlot);
            receivedGift2.senderTeam.Should().Be(receivedGift2.receiverTeam);
        }

        [Test]
        public void TestCheckGiftsDoesNotEmptyGiftBox()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            var gift = NewGiftItem();
            Wait();
            var result = _serviceSender.SendGift(gift, ReceiverName);
            Wait();

            // Assume
            result.Success.Should().BeTrue();

            // Act
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(1);

            // Assert
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(1);
            var (receivedGiftId, receivedGift) = gifts.First();
            receivedGiftId.Should().Be(result.GiftId);
            receivedGift.id.Should().Be(result.GiftId);
            receivedGift.itemName.Should().Be(gift.Name);
            receivedGift.amount.Should().Be(gift.Amount);
            receivedGift.itemValue.Should().Be(gift.Value);
            receivedGift.senderSlot.Should().Be(_testSessions.SenderSlot);
            receivedGift.receiverSlot.Should().Be(_testSessions.ReceiverSlot);
            receivedGift.senderTeam.Should().Be(receivedGift.receiverTeam);
        }

        [Test]
        public void TestCheckGiftsAndEmptyEmptiesGiftBox()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            var gift = NewGiftItem();
            Wait();
            var result = _serviceSender.SendGift(gift, ReceiverName);
            Wait();

            // Assume
            result.Success.Should().BeTrue();

            // Act
            var gifts = _serviceReceiver.GetAllGiftsAndEmptyGiftbox();

            // Assert
            gifts.Should().NotBeNull().And.HaveCount(1);
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.BeEmpty();
        }

        [Test]
        public void TestRemoveGiftRemovesJustCorrectIdFromGiftBox()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            var gift1 = NewGiftItem();
            var gift2 = NewGiftItem();
            Wait();
            var result1 = _serviceSender.SendGift(gift1, ReceiverName);
            var result2 = _serviceSender.SendGift(gift2, ReceiverName);
            Wait();

            // Assume
            result1.Success.Should().BeTrue();
            result2.Success.Should().BeTrue();
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(2);

            // Act
            _serviceReceiver.RemoveGiftFromGiftBox(result2.GiftId);
            Wait();

            // Assert
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(1);
            gifts.Should().ContainKey(result1.GiftId);
        }

        [Test]
        public void TestRemoveGiftsRemovesJustCorrectIdsFromGiftBox()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            var gift1 = NewGiftItem();
            var gift2 = NewGiftItem();
            var gift3 = NewGiftItem();
            var gift4 = NewGiftItem();
            Wait();
            var result1 = _serviceSender.SendGift(gift1, ReceiverName);
            var result2 = _serviceSender.SendGift(gift2, ReceiverName);
            var result3 = _serviceSender.SendGift(gift3, ReceiverName);
            var result4 = _serviceSender.SendGift(gift4, ReceiverName);
            Wait();

            // Assume
            result1.Success.Should().BeTrue();
            result2.Success.Should().BeTrue();
            result3.Success.Should().BeTrue();
            result4.Success.Should().BeTrue();
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(4);

            // Act
            _serviceReceiver.RemoveGiftsFromGiftBox(new[] { result1.GiftId, result4.GiftId });
            Wait();

            // Assert
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(2);
            gifts.Should().ContainKey(result2.GiftId);
            gifts.Should().ContainKey(result3.GiftId);
        }

        [Test]
        public void TestSendGiftWithTraitsSucceeds()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            var gift = NewGiftItem();
            var traits = NewGiftTraits();
            Wait();

            // Assume
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(0);

            // Act
            var result = _serviceSender.SendGift(gift, traits, ReceiverName);
            Wait();

            // Assert
            result.Success.Should().BeTrue();
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(1);
            var receivedGift = gifts[result.GiftId];
            receivedGift.itemName.Should().Be(gift.Name);
            receivedGift.amount.Should().Be(gift.Amount);
            receivedGift.itemValue.Should().Be(gift.Value);
            var receivedTraits = receivedGift.traits;
            receivedTraits.Should().HaveCount(traits.Length);
            for (var i = 0; i < traits.Length; i++)
            {
                receivedTraits[i].trait.Should().BeEquivalentTo(traits[i].trait);
                receivedTraits[i].quality.Should().BeApproximately(traits[i].quality, 0.001);
                receivedTraits[i].duration.Should().BeApproximately(traits[i].duration, 0.001);
            }
        }

        [Test]
        public void TestSendTwoGiftsWithTraitsStacksGifts()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            var gift1 = NewGiftItem();
            var gift2 = NewGiftItem();
            var traits1 = NewGiftTraits();
            var traits2 = NewGiftTraits();
            Wait();

            // Assume
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull();
            gifts.Should().HaveCount(0);

            // Act
            var result1 = _serviceSender.SendGift(gift1, traits1, ReceiverName);
            var result2 = _serviceSender.SendGift(gift2, traits2, ReceiverName);
            Wait();

            // Assert
            result1.Success.Should().BeTrue();
            result2.Success.Should().BeTrue();
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull();
            gifts.Should().HaveCount(2);
            var receivedGift1 = gifts[result1.GiftId];
            receivedGift1.itemName.Should().Be(gift1.Name);
            receivedGift1.amount.Should().Be(gift1.Amount);
            receivedGift1.itemValue.Should().Be(gift1.Value);
            var receivedTraits1 = receivedGift1.traits;
            receivedTraits1.Should().HaveCount(traits1.Length);
            for (var i = 0; i < traits1.Length; i++)
            {
                receivedTraits1[i].trait.Should().BeEquivalentTo(traits1[i].trait);
                receivedTraits1[i].quality.Should().BeApproximately(traits1[i].quality, 0.001);
                receivedTraits1[i].duration.Should().BeApproximately(traits1[i].duration, 0.001);
            }
            var receivedGift2 = gifts[result2.GiftId];
            receivedGift2.itemName.Should().Be(gift2.Name);
            receivedGift2.amount.Should().Be(gift2.Amount);
            receivedGift2.itemValue.Should().Be(gift2.Value);
            var receivedTraits2 = receivedGift2.traits;
            receivedTraits2.Should().HaveCount(traits2.Length);
            for (var i = 0; i < traits2.Length; i++)
            {
                receivedTraits2[i].trait.Should().BeEquivalentTo(traits2[i].trait);
                receivedTraits2[i].quality.Should().BeApproximately(traits2[i].quality, 0.001);
                receivedTraits2[i].duration.Should().BeApproximately(traits2[i].duration, 0.001);
            }
        }

        [Test]
        public void TestRefundGiftSendsItToSender()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            _serviceSender.OpenGiftBox();
            var gift = NewGiftItem();
            var traits = NewGiftTraits();
            Wait();
            var result = _serviceSender.SendGift(gift, traits, ReceiverName);
            Wait();

            // Assume
            result.Success.Should().BeTrue();
            var giftsReceiver = _serviceReceiver.CheckGiftBox();
            giftsReceiver.Should().NotBeNull();
            giftsReceiver.Should().HaveCount(1);
            var giftReceiver = giftsReceiver[result.GiftId];
            giftReceiver.isRefund.Should().BeFalse();
            giftReceiver.itemName.Should().Be(gift.Name);
            giftReceiver.senderSlot.Should().Be(_testSessions.SenderSlot);
            giftReceiver.receiverSlot.Should().Be(_testSessions.ReceiverSlot);
            giftReceiver.senderTeam.Should().Be(giftReceiver.receiverTeam);
            var giftsSender = _serviceSender.CheckGiftBox();
            giftsSender.Should().BeEmpty();

            // Act
            giftsReceiver = _serviceReceiver.GetAllGiftsAndEmptyGiftbox();
            result = _serviceReceiver.RefundGift(giftsReceiver[result.GiftId]);
            Wait();

            // Assert
            result.Success.Should().BeTrue();
            giftsReceiver = _serviceReceiver.CheckGiftBox();
            giftsReceiver.Should().BeEmpty();
            giftsSender = _serviceSender.CheckGiftBox();
            giftsSender.Should().NotBeNull().And.HaveCount(1);
            var giftSender = giftsSender[result.GiftId];
            giftSender.isRefund.Should().BeTrue();
            giftSender.itemName.Should().Be(gift.Name);
            giftSender.itemName.Should().Be(gift.Name);
            giftSender.senderSlot.Should().Be(_testSessions.SenderSlot);
            giftSender.receiverSlot.Should().Be(_testSessions.ReceiverSlot);
        }

        [Test]
        public void TestSendGiftToAlias()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            Wait();
            SetAlias(_sessionReceiver, "receiver2");
            var gift = NewGiftItem();

            // Assume
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().BeEmpty();

            // Act
            var result = _serviceSender.SendGift(gift, "receiver2");
            Wait();

            // Assert
            result.Success.Should().BeTrue();
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(1);
            var (receivedGiftId, receivedGift) = gifts.First();
            receivedGiftId.Should().Be(result.GiftId);
            receivedGift.id.Should().Be(result.GiftId);
            receivedGift.itemName.Should().Be(gift.Name);
            receivedGift.amount.Should().Be(gift.Amount);
            receivedGift.itemValue.Should().Be(gift.Value);
            receivedGift.senderSlot.Should().Be(_testSessions.SenderSlot);
            receivedGift.receiverSlot.Should().Be(_testSessions.ReceiverSlot);
            receivedGift.senderTeam.Should().Be(receivedGift.receiverTeam);
        }

        [Test]
        public void TestSendGiftToNameEvenIfAliasExists()
        {
            // Arrange
            _serviceSender.OpenGiftBox();
            _serviceReceiver.OpenGiftBox();
            Wait();
            SetAlias(_sessionSender, ReceiverName);
            var gift = NewGiftItem();

            // Assume
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().BeEmpty();

            // Act
            var result = _serviceSender.SendGift(gift, ReceiverName);
            Wait();

            // Assert
            result.Success.Should().BeTrue();
            var giftsSender = _serviceSender.CheckGiftBox();
            giftsSender.Should().BeEmpty();
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(1);
            var (receivedGiftId, receivedGift) = gifts.First();
            receivedGiftId.Should().Be(result.GiftId);
            receivedGift.id.Should().Be(result.GiftId);
            receivedGift.itemName.Should().Be(gift.Name);
            receivedGift.amount.Should().Be(gift.Amount);
            receivedGift.itemValue.Should().Be(gift.Value);
            receivedGift.senderSlot.Should().Be(_testSessions.SenderSlot);
            receivedGift.receiverSlot.Should().Be(_testSessions.ReceiverSlot);
            receivedGift.senderTeam.Should().Be(receivedGift.receiverTeam);
        }

        [Test]
        public void TestCanSubscribeAndGetNotified()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            Wait();
            var giftItem = NewGiftItem();
            var hasBeenNotified = false;
            _serviceReceiver.SubscribeToNewGifts((gifts) =>
            {
                hasBeenNotified = true;
                gifts.Should().NotBeNull();
                gifts.Should().HaveCount(1);
                var notifiedGift = gifts.Values.First();
                notifiedGift.itemName.Should().Be(giftItem.Name);
                notifiedGift.amount.Should().Be(giftItem.Amount);
                notifiedGift.itemValue.Should().Be(giftItem.Value);
            });
            Wait();

            // Act
            _serviceSender.SendGift(giftItem, ReceiverName);
            Wait(300);

            // Assert
            hasBeenNotified.Should().BeTrue();
        }

        [Test]
        public void TestSendGiftWithDefaultQualitiesAndDurationsOmitsThemFromDataStorage()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            var gift = NewGiftItem();
            var traits = new GiftTrait[] { new("Food"), new("Drink", 2.0), new("Heal", 1.0, 0.0), new("Damage", 1.1, -1.0) };
            Wait();

            // Assume
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().BeEmpty();

            // Act
            var result = _serviceSender.SendGift(gift, traits, ReceiverName);
            Wait();

            // Assert
            result.Success.Should().BeTrue();

            var giftboxKey = $"GiftBox;{_testSessions.ReceiverTeam};{_testSessions.ReceiverSlot}";
            var content = _sessionReceiver.DataStorage[Scope.Global, giftboxKey].To<JToken>().ToString(Formatting.None);
            content.Should().NotBeNullOrWhiteSpace();
            content.Should().Contain("{\"trait\":\"Food\"}");
            content.Should().Contain("{\"trait\":\"Drink\",\"quality\":2.0}");
            content.Should().Contain("{\"trait\":\"Heal\",\"duration\":0.0}");
            content.Should().Contain("{\"trait\":\"Damage\",\"quality\":1.1,\"duration\":-1.0}");

            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(1);
            var (receivedGiftId, receivedGift) = gifts.First();
            receivedGiftId.Should().Be(result.GiftId);
            receivedGift.id.Should().Be(result.GiftId);
            receivedGift.itemName.Should().Be(gift.Name);
            receivedGift.amount.Should().Be(gift.Amount);
            receivedGift.itemValue.Should().Be(gift.Value);
            receivedGift.senderSlot.Should().Be(_testSessions.SenderSlot);
            receivedGift.receiverSlot.Should().Be(_testSessions.ReceiverSlot);
            receivedGift.senderTeam.Should().Be(receivedGift.receiverTeam);
            receivedGift.traits[0].trait.Should().Be("Food");
            receivedGift.traits[0].quality.Should().BeApproximately(1.0, 0.1);
            receivedGift.traits[0].duration.Should().BeApproximately(1.0, 0.1);
            receivedGift.traits[1].trait.Should().Be("Drink");
            receivedGift.traits[1].quality.Should().BeApproximately(2.0, 0.1);
            receivedGift.traits[1].duration.Should().BeApproximately(1.0, 0.1);
            receivedGift.traits[2].trait.Should().Be("Heal");
            receivedGift.traits[2].quality.Should().BeApproximately(1.0, 0.1);
            receivedGift.traits[2].duration.Should().BeApproximately(0.0, 0.1);
            receivedGift.traits[3].trait.Should().Be("Damage");
            receivedGift.traits[3].quality.Should().BeApproximately(1.1, 0.1);
            receivedGift.traits[3].duration.Should().BeApproximately(-1.0, 0.1);
        }
    }
}