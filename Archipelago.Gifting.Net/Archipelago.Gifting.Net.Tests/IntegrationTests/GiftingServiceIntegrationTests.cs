using Archipelago.Gifting.Net.Traits;
using Archipelago.Gifting.Net.Versioning.Gifts;
using Archipelago.Gifting.Net.Versioning.Gifts.Current;
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
            WaitShort();

            // Assume

            // Act
            var canGift = _serviceSender.CanGiftToPlayer(ReceiverName);

            // Assert
            canGift.Should().BeFalse();
        }

        [Test]
        public void TestOpenGiftBoxRegistersAsCanSendGift()
        {
            // Arrange

            // Assume
            var canGift = _serviceSender.CanGiftToPlayer(ReceiverName);
            canGift.Should().BeFalse();

            // Act
            _serviceReceiver.OpenGiftBox();
            WaitShort();

            // Assert
            canGift = _serviceSender.CanGiftToPlayer(ReceiverName);
            canGift.Should().BeTrue();
        }

        [Test]
        public void TestCannotSendGiftToNonExistingPlayer()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            WaitShort();

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
            WaitShort();
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
            WaitShort();
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
            WaitShort();
            SetAlias(_sessionSender, ReceiverName);
            SetAlias(_sessionReceiver, SenderName);

            // Assume

            // Act
            var canGift = _serviceSender.CanGiftToPlayer(ReceiverName);

            // Assert
            canGift.Should().BeTrue();
        }

        [Test]
        public void TestOpenGiftBoxCreatesEmptyGiftDictionary()
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
        public void TestCloseGiftBoxShouldCannotSendGiftsAnymore()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            WaitShort();

            // Assume
            var canGift = _serviceSender.CanGiftToPlayer(ReceiverName);
            canGift.Should().BeTrue();

            // Act
            _serviceReceiver.CloseGiftBox();
            WaitShort();

            // Assert
            canGift = _serviceSender.CanGiftToPlayer(ReceiverName);
            canGift.Should().BeFalse();
        }

        [Test]
        public void TestCloseGiftBoxShouldNotTurnGiftsToNull()
        {
            // Arrange
            _serviceSender.OpenGiftBox();
            WaitShort();

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
            WaitShort();

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
            WaitShort();

            // Assume
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().BeEmpty();

            // Act
            var result = _serviceSender.SendGift(gift, ReceiverName);
            WaitShort();

            // Assert
            result.Success.Should().BeTrue();
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(1);
            var (receivedGiftId, receivedGift) = gifts.First();
            receivedGiftId.Should().Be(result.GiftId);
            receivedGift.ID.Should().Be(result.GiftId);
            receivedGift.ItemName.Should().Be(gift.Name);
            receivedGift.Amount.Should().Be(gift.Amount);
            receivedGift.ItemValue.Should().Be(gift.Value);
            receivedGift.SenderSlot.Should().Be(_testSessions.SenderSlot);
            receivedGift.ReceiverSlot.Should().Be(_testSessions.ReceiverSlot);
            receivedGift.SenderTeam.Should().Be(receivedGift.ReceiverTeam);
        }

        [Test]
        public void TestSendTwoGiftToOpenBoxStacksGifts()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            var gift1 = NewGiftItem();
            var gift2 = NewGiftItem();
            WaitShort();

            // Assume
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().BeEmpty();

            // Act
            var result1 = _serviceSender.SendGift(gift1, ReceiverName);
            var result2 = _serviceSender.SendGift(gift2, ReceiverName);
            WaitShort();

            // Assert
            result1.Success.Should().BeTrue();
            result2.Success.Should().BeTrue();
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(2);
            var receivedGift1 = gifts[result1.GiftId];
            var receivedGift2 = gifts[result2.GiftId];
            receivedGift1.ItemName.Should().Be(gift1.Name);
            receivedGift1.Amount.Should().Be(gift1.Amount);
            receivedGift1.ItemValue.Should().Be(gift1.Value);
            receivedGift1.SenderSlot.Should().Be(_testSessions.SenderSlot);
            receivedGift1.ReceiverSlot.Should().Be(_testSessions.ReceiverSlot);
            receivedGift1.SenderTeam.Should().Be(receivedGift1.ReceiverTeam);
            receivedGift2.ItemName.Should().Be(gift2.Name);
            receivedGift2.Amount.Should().Be(gift2.Amount);
            receivedGift2.ItemValue.Should().Be(gift2.Value);
            receivedGift2.SenderSlot.Should().Be(_testSessions.SenderSlot);
            receivedGift2.ReceiverSlot.Should().Be(_testSessions.ReceiverSlot);
            receivedGift2.SenderTeam.Should().Be(receivedGift2.ReceiverTeam);
        }

        [Test]
        public void TestCheckGiftsDoesNotEmptyGiftBox()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            var gift = NewGiftItem();
            WaitShort();
            var result = _serviceSender.SendGift(gift, ReceiverName);
            WaitShort();

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
            receivedGift.ID.Should().Be(result.GiftId);
            receivedGift.ItemName.Should().Be(gift.Name);
            receivedGift.Amount.Should().Be(gift.Amount);
            receivedGift.ItemValue.Should().Be(gift.Value);
            receivedGift.SenderSlot.Should().Be(_testSessions.SenderSlot);
            receivedGift.ReceiverSlot.Should().Be(_testSessions.ReceiverSlot);
            receivedGift.SenderTeam.Should().Be(receivedGift.ReceiverTeam);
        }

        [Test]
        public void TestCheckGiftsAndEmptyEmptiesGiftBox()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            var gift = NewGiftItem();
            WaitShort();
            var result = _serviceSender.SendGift(gift, ReceiverName);
            WaitShort();

            // Assume
            result.Success.Should().BeTrue();

            // Act
            var gifts = _serviceReceiver.GetAllGiftsAndEmptyGiftBox();

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
            WaitShort();
            var result1 = _serviceSender.SendGift(gift1, ReceiverName);
            var result2 = _serviceSender.SendGift(gift2, ReceiverName);
            WaitShort();

            // Assume
            result1.Success.Should().BeTrue();
            result2.Success.Should().BeTrue();
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(2);

            // Act
            _serviceReceiver.RemoveGiftFromGiftBox(result2.GiftId);
            WaitShort();

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
            WaitShort();
            var result1 = _serviceSender.SendGift(gift1, ReceiverName);
            var result2 = _serviceSender.SendGift(gift2, ReceiverName);
            var result3 = _serviceSender.SendGift(gift3, ReceiverName);
            var result4 = _serviceSender.SendGift(gift4, ReceiverName);
            WaitShort();

            // Assume
            result1.Success.Should().BeTrue();
            result2.Success.Should().BeTrue();
            result3.Success.Should().BeTrue();
            result4.Success.Should().BeTrue();
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(4);

            // Act
            _serviceReceiver.RemoveGiftsFromGiftBox(new[] { result1.GiftId, result4.GiftId });
            WaitShort();

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
            WaitShort();

            // Assume
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(0);

            // Act
            var result = _serviceSender.SendGift(gift, traits, ReceiverName);
            WaitShort();

            // Assert
            result.Success.Should().BeTrue();
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(1);
            var receivedGift = gifts[result.GiftId];
            receivedGift.ItemName.Should().Be(gift.Name);
            receivedGift.Amount.Should().Be(gift.Amount);
            receivedGift.ItemValue.Should().Be(gift.Value);
            var receivedTraits = receivedGift.Traits;
            receivedTraits.Should().HaveCount(traits.Length);
            for (var i = 0; i < traits.Length; i++)
            {
                receivedTraits[i].Trait.Should().BeEquivalentTo(traits[i].Trait);
                receivedTraits[i].Quality.Should().BeApproximately(traits[i].Quality, 0.001);
                receivedTraits[i].Duration.Should().BeApproximately(traits[i].Duration, 0.001);
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
            WaitShort();

            // Assume
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull();
            gifts.Should().HaveCount(0);

            // Act
            var result1 = _serviceSender.SendGift(gift1, traits1, ReceiverName);
            var result2 = _serviceSender.SendGift(gift2, traits2, ReceiverName);
            WaitShort();

            // Assert
            result1.Success.Should().BeTrue();
            result2.Success.Should().BeTrue();
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull();
            gifts.Should().HaveCount(2);
            var receivedGift1 = gifts[result1.GiftId];
            receivedGift1.ItemName.Should().Be(gift1.Name);
            receivedGift1.Amount.Should().Be(gift1.Amount);
            receivedGift1.ItemValue.Should().Be(gift1.Value);
            var receivedTraits1 = receivedGift1.Traits;
            receivedTraits1.Should().HaveCount(traits1.Length);
            for (var i = 0; i < traits1.Length; i++)
            {
                receivedTraits1[i].Trait.Should().BeEquivalentTo(traits1[i].Trait);
                receivedTraits1[i].Quality.Should().BeApproximately(traits1[i].Quality, 0.001);
                receivedTraits1[i].Duration.Should().BeApproximately(traits1[i].Duration, 0.001);
            }
            var receivedGift2 = gifts[result2.GiftId];
            receivedGift2.ItemName.Should().Be(gift2.Name);
            receivedGift2.Amount.Should().Be(gift2.Amount);
            receivedGift2.ItemValue.Should().Be(gift2.Value);
            var receivedTraits2 = receivedGift2.Traits;
            receivedTraits2.Should().HaveCount(traits2.Length);
            for (var i = 0; i < traits2.Length; i++)
            {
                receivedTraits2[i].Trait.Should().BeEquivalentTo(traits2[i].Trait);
                receivedTraits2[i].Quality.Should().BeApproximately(traits2[i].Quality, 0.001);
                receivedTraits2[i].Duration.Should().BeApproximately(traits2[i].Duration, 0.001);
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
            WaitShort();
            var result = _serviceSender.SendGift(gift, traits, ReceiverName);
            WaitShort();

            // Assume
            result.Success.Should().BeTrue();
            var giftsReceiver = _serviceReceiver.CheckGiftBox();
            giftsReceiver.Should().NotBeNull();
            giftsReceiver.Should().HaveCount(1);
            var giftReceiver = giftsReceiver[result.GiftId];
            giftReceiver.IsRefund.Should().BeFalse();
            giftReceiver.ItemName.Should().Be(gift.Name);
            giftReceiver.SenderSlot.Should().Be(_testSessions.SenderSlot);
            giftReceiver.ReceiverSlot.Should().Be(_testSessions.ReceiverSlot);
            giftReceiver.SenderTeam.Should().Be(giftReceiver.ReceiverTeam);
            var giftsSender = _serviceSender.CheckGiftBox();
            giftsSender.Should().BeEmpty();

            // Act
            giftsReceiver = _serviceReceiver.GetAllGiftsAndEmptyGiftBox();
            result = _serviceReceiver.RefundGift(giftsReceiver[result.GiftId]);
            WaitShort();

            // Assert
            result.Success.Should().BeTrue();
            giftsReceiver = _serviceReceiver.CheckGiftBox();
            giftsReceiver.Should().BeEmpty();
            giftsSender = _serviceSender.CheckGiftBox();
            giftsSender.Should().NotBeNull().And.HaveCount(1);
            var giftSender = giftsSender[result.GiftId];
            giftSender.IsRefund.Should().BeTrue();
            giftSender.ItemName.Should().Be(gift.Name);
            giftSender.ItemName.Should().Be(gift.Name);
            giftSender.SenderSlot.Should().Be(_testSessions.SenderSlot);
            giftSender.ReceiverSlot.Should().Be(_testSessions.ReceiverSlot);
        }

        [Test]
        public void TestSendGiftToAlias()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            WaitShort();
            SetAlias(_sessionReceiver, "receiver2");
            var gift = NewGiftItem();

            // Assume
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().BeEmpty();

            // Act
            var result = _serviceSender.SendGift(gift, "receiver2");
            WaitShort();

            // Assert
            result.Success.Should().BeTrue();
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(1);
            var (receivedGiftId, receivedGift) = gifts.First();
            receivedGiftId.Should().Be(result.GiftId);
            receivedGift.ID.Should().Be(result.GiftId);
            receivedGift.ItemName.Should().Be(gift.Name);
            receivedGift.Amount.Should().Be(gift.Amount);
            receivedGift.ItemValue.Should().Be(gift.Value);
            receivedGift.SenderSlot.Should().Be(_testSessions.SenderSlot);
            receivedGift.ReceiverSlot.Should().Be(_testSessions.ReceiverSlot);
            receivedGift.SenderTeam.Should().Be(receivedGift.ReceiverTeam);
        }

        [Test]
        public void TestSendGiftToNameEvenIfAliasExists()
        {
            // Arrange
            _serviceSender.OpenGiftBox();
            _serviceReceiver.OpenGiftBox();
            WaitShort();
            SetAlias(_sessionSender, ReceiverName);
            var gift = NewGiftItem();

            // Assume
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().BeEmpty();

            // Act
            var result = _serviceSender.SendGift(gift, ReceiverName);
            WaitShort();

            // Assert
            result.Success.Should().BeTrue();
            var giftsSender = _serviceSender.CheckGiftBox();
            giftsSender.Should().BeEmpty();
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(1);
            var (receivedGiftId, receivedGift) = gifts.First();
            receivedGiftId.Should().Be(result.GiftId);
            receivedGift.ID.Should().Be(result.GiftId);
            receivedGift.ItemName.Should().Be(gift.Name);
            receivedGift.Amount.Should().Be(gift.Amount);
            receivedGift.ItemValue.Should().Be(gift.Value);
            receivedGift.SenderSlot.Should().Be(_testSessions.SenderSlot);
            receivedGift.ReceiverSlot.Should().Be(_testSessions.ReceiverSlot);
            receivedGift.SenderTeam.Should().Be(receivedGift.ReceiverTeam);
        }

        [Test]
        public async Task TestCanSubscribeAndGetNotifiedOnce()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            WaitShort();
            var giftItem = NewGiftItem();
            var hasBeenNotified = 0;

            void NewGiftsCallback(Gift gift)
            {
                AssertGiftAndIncrement(ref hasBeenNotified, hasBeenNotified, gift, giftItem);
            }

            _serviceReceiver.OnNewGift += NewGiftsCallback;
            WaitMedium();

            // Act
            _serviceSender.SendGift(giftItem, ReceiverName);
            await WaitMediumAsync();

            // Assert
            hasBeenNotified.Should().Be(1);
        }

        [Test]
        public async Task TestCanSubscribeAndGetNotifiedThreeTimes()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            WaitShort();
            var giftItem1 = NewGiftItem();
            var giftItem2 = NewGiftItem();
            var giftItem3 = NewGiftItem();
            var giftItems = new []{giftItem1, giftItem2, giftItem3};
            var hasBeenNotified = 0;

            void NewGiftsCallback(Gift gift)
            {
                AssertGiftAndIncrement(ref hasBeenNotified, hasBeenNotified, gift, giftItems[hasBeenNotified]);
            }

            _serviceReceiver.OnNewGift += NewGiftsCallback;
            WaitMedium();

            // Act
            _serviceSender.SendGift(giftItem1, ReceiverName);
            await WaitMediumAsync();

            hasBeenNotified.Should().Be(1);

            _serviceSender.SendGift(giftItem2, ReceiverName);
            _serviceSender.SendGift(giftItem3, ReceiverName);
            await WaitMediumAsync();

            // Assert
            hasBeenNotified.Should().Be(3);
        }

        [Test]
        public async Task TestCanUnsubscribe()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            WaitShort();
            var giftItem1 = NewGiftItem();
            var giftItem2 = NewGiftItem();
            var giftItems = new[] { giftItem1, giftItem2 };
            var hasBeenNotified = 0;

            void NewGiftsCallback(Gift gift)
            {
                Console.WriteLine($"5 - {DateTime.Now.Second}.{DateTime.Now.Millisecond}");
                AssertGiftAndIncrement(ref hasBeenNotified, 0, gift, giftItems[0]);
                Console.WriteLine($"6 - {DateTime.Now.Second}.{DateTime.Now.Millisecond}");
            }

            Console.WriteLine($"1 - {DateTime.Now.Second}.{DateTime.Now.Millisecond}");
            _serviceReceiver.OnNewGift += NewGiftsCallback;
            Console.WriteLine($"2 - {DateTime.Now.Second}.{DateTime.Now.Millisecond}");
            WaitMedium();
            Console.WriteLine($"3 - {DateTime.Now.Second}.{DateTime.Now.Millisecond}");

            // Act
            _serviceSender.SendGift(giftItem1, ReceiverName);
            Console.WriteLine($"4 - {DateTime.Now.Second}.{DateTime.Now.Millisecond}");
            await WaitMediumAsync();
            Console.WriteLine($"7 - {DateTime.Now.Second}.{DateTime.Now.Millisecond}");
            hasBeenNotified.Should().Be(1);
            Console.WriteLine($"8 - {DateTime.Now.Second}.{DateTime.Now.Millisecond}");

            _serviceReceiver.OnNewGift -= NewGiftsCallback;
            WaitShort();

            _serviceSender.SendGift(giftItem2, ReceiverName);
            WaitMedium();

            // Assert
            hasBeenNotified.Should().Be(1);
        }

        [Test]
        public void TestCanHaveMultipleSubscribedHandlers()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            WaitShort();
            var giftItem1 = NewGiftItem();
            var giftItem2 = NewGiftItem();
            var giftItem3 = NewGiftItem();
            var giftItem4 = NewGiftItem();
            var callback1Count = 0;
            var callback2Count = 0;
            var callback3Count = 0;

            void NewGiftsCallback1(Gift gift)
            {
                AssertGiftAndIncrement(ref callback1Count, callback1Count, gift);
            }

            void NewGiftsCallback2(Gift gift)
            {
                AssertGiftAndIncrement(ref callback2Count, callback2Count, gift);
            }

            void NewGiftsCallback3(Gift gift)
            {
                AssertGiftAndIncrement(ref callback3Count, callback3Count, gift);
            }

            // Act
            _serviceReceiver.OnNewGift += NewGiftsCallback1;
            WaitMedium();

            _serviceSender.SendGift(giftItem1, ReceiverName);
            WaitMedium();
            callback1Count.Should().Be(1);
            callback2Count.Should().Be(0);
            callback3Count.Should().Be(0);

            _serviceReceiver.OnNewGift += NewGiftsCallback2;
            _serviceSender.SendGift(giftItem2, ReceiverName);
            WaitMedium();
            callback1Count.Should().Be(2);
            callback2Count.Should().Be(1);
            callback3Count.Should().Be(0);

            _serviceReceiver.OnNewGift += NewGiftsCallback3;
            _serviceReceiver.OnNewGift -= NewGiftsCallback1;

            _serviceSender.SendGift(giftItem3, ReceiverName);
            WaitMedium();
            callback1Count.Should().Be(2);
            callback2Count.Should().Be(2);
            callback3Count.Should().Be(1);

            _serviceReceiver.OnNewGift -= NewGiftsCallback3;
            _serviceReceiver.OnNewGift -= NewGiftsCallback2;

            _serviceSender.SendGift(giftItem4, ReceiverName);
            WaitMedium();

            // Assert
            callback1Count.Should().Be(2);
            callback2Count.Should().Be(2);
            callback3Count.Should().Be(1);
        }

        [Test]
        public void TestSendGiftWithDefaultQualitiesAndDurationsOmitsThemFromDataStorage()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            var gift = NewGiftItem();
            gift.Value = 0;
            var traits = new GiftTrait[] { new("Food"), new("Drink", 2.0), new("Heal", 1.0, 0.0), new("Damage", 1.1, -1.0) };
            WaitShort();

            // Assume
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().BeEmpty();

            // Act
            var result = _serviceSender.SendGift(gift, traits, ReceiverName);
            WaitShort();

            // Assert
            result.Success.Should().BeTrue();

            var giftBoxKey = $"GiftBox;{_testSessions.ReceiverTeam};{_testSessions.ReceiverSlot}";
            var content = _sessionReceiver.DataStorage[Scope.Global, giftBoxKey].To<JToken>().ToString(Formatting.None);
            content.Should().NotBeNullOrWhiteSpace();
            content.Should().Contain("{\"trait\":\"Food\"}");
            content.Should().Contain("{\"trait\":\"Drink\",\"quality\":2.0}");
            content.Should().Contain("{\"trait\":\"Heal\",\"duration\":0.0}");
            content.Should().Contain("{\"trait\":\"Damage\",\"quality\":1.1,\"duration\":-1.0}");
            content.Should().NotContain("item_value");
            content.Should().NotContain("itemValue");
            content.Should().NotContain("ItemValue");

            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(1);
            var (receivedGiftId, receivedGift) = gifts.First();
            receivedGiftId.Should().Be(result.GiftId);
            receivedGift.ID.Should().Be(result.GiftId);
            receivedGift.ItemName.Should().Be(gift.Name);
            receivedGift.Amount.Should().Be(gift.Amount);
            receivedGift.ItemValue.Should().Be(gift.Value);
            receivedGift.SenderSlot.Should().Be(_testSessions.SenderSlot);
            receivedGift.ReceiverSlot.Should().Be(_testSessions.ReceiverSlot);
            receivedGift.SenderTeam.Should().Be(receivedGift.ReceiverTeam);
            receivedGift.Traits[0].Trait.Should().Be("Food");
            receivedGift.Traits[0].Quality.Should().BeApproximately(1.0, 0.1);
            receivedGift.Traits[0].Duration.Should().BeApproximately(1.0, 0.1);
            receivedGift.Traits[1].Trait.Should().Be("Drink");
            receivedGift.Traits[1].Quality.Should().BeApproximately(2.0, 0.1);
            receivedGift.Traits[1].Duration.Should().BeApproximately(1.0, 0.1);
            receivedGift.Traits[2].Trait.Should().Be("Heal");
            receivedGift.Traits[2].Quality.Should().BeApproximately(1.0, 0.1);
            receivedGift.Traits[2].Duration.Should().BeApproximately(0.0, 0.1);
            receivedGift.Traits[3].Trait.Should().Be("Damage");
            receivedGift.Traits[3].Quality.Should().BeApproximately(1.1, 0.1);
            receivedGift.Traits[3].Duration.Should().BeApproximately(-1.0, 0.1);
        }

        [Test]
        public void TestGiftPropertiesSerializeToSnakeCase()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            var gift = NewGiftItem();
            var traits = new GiftTrait[] { new("Food") };
            WaitShort();

            // Assume
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().BeEmpty();

            // Act
            var result = _serviceSender.SendGift(gift, traits, ReceiverName);
            WaitShort();

            // Assert
            result.Success.Should().BeTrue();

            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(1);
            var (receivedGiftId, receivedGift) = gifts.First();

            var giftBoxKey = $"GiftBox;{_testSessions.ReceiverTeam};{_testSessions.ReceiverSlot}";
            var content = _sessionReceiver.DataStorage[Scope.Global, giftBoxKey].To<JToken>().ToString(Formatting.None);
            content.Should().NotBeNullOrWhiteSpace();
            content.Should().Contain($"\"id\":\"{receivedGiftId}\"");
            content.Should().Contain($"\"item_name\":\"{gift.Name}\"");
            content.Should().Contain($"\"amount\":{gift.Amount}");
            content.Should().Contain($"\"item_value\":{gift.Value}");
            content.Should().Contain($"\"traits\":");
            content.Should().Contain($"\"sender_slot\":{_testSessions.SenderSlot}");
            content.Should().Contain($"\"receiver_slot\":{_testSessions.ReceiverSlot}");
            content.Should().Contain($"\"sender_team\":{_testSessions.SenderTeam}");
            content.Should().Contain($"\"receiver_team\":{_testSessions.ReceiverTeam}");
            content.Should().Contain("\"is_refund\":false");
            content.Should().NotContain("ID");
            content.Should().NotContain("ItemName").And.NotContain("itemName");
            content.Should().NotContain("Amount");
            content.Should().NotContain("ItemValue").And.NotContain("itemValue");
            content.Should().NotContain("Traits");
            content.Should().NotContain("SenderSlot").And.NotContain("senderSlot");
            content.Should().NotContain("ReceiverSlot").And.NotContain("receiverSlot");
            content.Should().NotContain("SenderTeam").And.NotContain("senderTeam");
            content.Should().NotContain("ReceiverTeam").And.NotContain("receiverTeam");
            content.Should().NotContain("IsRefund").And.NotContain("isRefund");

            receivedGiftId.Should().Be(result.GiftId);
            receivedGift.ID.Should().Be(result.GiftId);
            receivedGift.ItemName.Should().Be(gift.Name);
            receivedGift.Amount.Should().Be(gift.Amount);
            receivedGift.ItemValue.Should().Be(gift.Value);
            receivedGift.SenderSlot.Should().Be(_testSessions.SenderSlot);
            receivedGift.ReceiverSlot.Should().Be(_testSessions.ReceiverSlot);
            receivedGift.SenderTeam.Should().Be(receivedGift.SenderTeam);
            receivedGift.ReceiverTeam.Should().Be(receivedGift.ReceiverTeam);
            receivedGift.IsRefund.Should().Be(false);
        }

        private static void AssertGiftAndIncrement(ref int hasBeenNotified, int expectedNotified, Gift gift, GiftItem? expectedGiftItem = null)
        {
            try
            {
                hasBeenNotified.Should().Be(expectedNotified);
                gift.Should().NotBeNull();
                if (expectedGiftItem != null)
                {
                    gift.ItemName.Should().Be(expectedGiftItem.Name);
                    gift.Amount.Should().Be(expectedGiftItem.Amount);
                    gift.ItemValue.Should().Be(expectedGiftItem.Value);
                }

                hasBeenNotified += 1;
            }
            catch (Exception ex)
            {
                hasBeenNotified = -1;
                Console.WriteLine(ex);
                throw;
            }
        }

        #region Obsolete Stuff

        [Test]
        [Obsolete("Testing the old notification implementation")]
        public async Task TestCanSubscribeAndGetNotifiedObsolete()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            WaitShort();
            var giftItem = NewGiftItem();
            var hasBeenNotified = false;
            _serviceReceiver.SubscribeToNewGifts((gifts) =>
            {
                Console.WriteLine($"4 - {DateTime.Now.Second}.{DateTime.Now.Millisecond}");
                hasBeenNotified = true;
                gifts.Should().NotBeNull();
                gifts.Should().HaveCount(1);
                var notifiedGift = gifts.Values.First();
                notifiedGift.ItemName.Should().Be(giftItem.Name);
                notifiedGift.Amount.Should().Be(giftItem.Amount);
                notifiedGift.ItemValue.Should().Be(giftItem.Value);
                Console.WriteLine($"5 - {DateTime.Now.Second}.{DateTime.Now.Millisecond}");
            });
            Console.WriteLine($"1 - {DateTime.Now.Second}.{DateTime.Now.Millisecond}");
            WaitMedium();
            Console.WriteLine($"2 - {DateTime.Now.Second}.{DateTime.Now.Millisecond}");

            // Act
            Console.WriteLine($"3 - {DateTime.Now.Second}.{DateTime.Now.Millisecond}");
            _serviceSender.SendGift(giftItem, ReceiverName);
            Console.WriteLine($"6 - {DateTime.Now.Second}.{DateTime.Now.Millisecond}");
            await WaitMediumAsync();
            Console.WriteLine($"7 - {DateTime.Now.Second}.{DateTime.Now.Millisecond}");

            // Assert
            hasBeenNotified.Should().BeTrue();
        }

        #endregion Obsolete Stuff
    }
}