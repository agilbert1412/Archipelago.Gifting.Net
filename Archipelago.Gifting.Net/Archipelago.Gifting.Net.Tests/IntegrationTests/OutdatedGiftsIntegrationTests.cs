using Archipelago.Gifting.Net.Giftboxes;
using Archipelago.Gifting.Net.Gifts;
using Archipelago.Gifting.Net.Gifts.Versions;
using Archipelago.Gifting.Net.Service;
using Archipelago.Gifting.Net.Traits;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using FluentAssertions;

namespace Archipelago.Gifting.Net.Tests.IntegrationTests
{
    public class OutdatedGiftsIntegrationTests : IntegrationTestBase
    {
        private Random _random = new Random(1234);

        [Test]
        public void TestCanReadOutdatedGift()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            var giftItem = NewGiftItem();
            Wait();
            SendVersion1Gift(giftItem, new GiftTrait[0], out var giftId);
            Wait();

            // Act
            var gifts = _serviceReceiver.CheckGiftBox();

            // Assert
            gifts.Should().NotBeNull().And.HaveCount(1);
            var (receivedGiftId, receivedGift) = gifts.First();
            receivedGiftId.Should().Be(giftId.ToString());
            receivedGift.ID.Should().Be(giftId.ToString());
            receivedGift.ItemName.Should().Be(giftItem.Name);
            receivedGift.Amount.Should().Be(giftItem.Amount);
            receivedGift.ItemValue.Should().Be(giftItem.Value);
            receivedGift.SenderSlot.Should().Be(_testSessions.SenderSlot);
            receivedGift.ReceiverSlot.Should().Be(_testSessions.ReceiverSlot);
            receivedGift.SenderTeam.Should().Be(receivedGift.ReceiverTeam);
        }

        [Test]
        public void TestCanCreateOutdatedGift()
        {
            // Arrange
            var outdatedGiftbox = new GiftBox(true)
            {
                MinimumGiftDataVersion = DataVersion.GIFT_DATA_VERSION_1,
                MaximumGiftDataVersion = DataVersion.GIFT_DATA_VERSION_1,
            };
            _serviceReceiver.UpdateGiftBox(outdatedGiftbox);
            var giftItem = NewGiftItem();
            Wait();

            // Act
            var success = _serviceSender.SendGift(giftItem, ReceiverName, out var giftId);
            Wait();

            // Assert
            success.Should().BeTrue();
            var existingGiftBox = _sessionReceiver.DataStorage[Scope.Global, $"GiftBox;{_testSessions.ReceiverTeam};{_testSessions.ReceiverSlot}"];
            var gifts = new Gifts.Versions.Version1.Converter().ReadFromDataStorage(existingGiftBox);
            gifts.Should().NotBeNull().And.HaveCount(1);
            var (receivedGiftId, receivedGift) = gifts.First();
            receivedGiftId.Should().Be(giftId);
            receivedGift.ID.Should().Be(giftId);
            receivedGift.Item.Should().NotBeNull();
            receivedGift.Item.Name.Should().Be(giftItem.Name);
            receivedGift.Item.Amount.Should().Be(giftItem.Amount);
            receivedGift.Item.Value.Should().Be(giftItem.Value);
            receivedGift.SenderName.Should().Be(SenderName);
            receivedGift.ReceiverName.Should().Be(ReceiverName);
            receivedGift.SenderTeam.Should().Be(receivedGift.ReceiverTeam);
        }

        [Test]
        public void TestCanCreateCurrentGiftForFutureGiftBoxThatIsBackwardCompatible()
        {
            // Arrange
            var outdatedGiftbox = new GiftBox(true)
            {
                MinimumGiftDataVersion = DataVersion.FirstVersion,
                MaximumGiftDataVersion = DataVersion.Current + 1,
            };
            _serviceReceiver.UpdateGiftBox(outdatedGiftbox);
            var giftItem = NewGiftItem();
            Wait();

            // Assume
            var canGift = _serviceSender.CanGiftToPlayer(ReceiverName);
            canGift.Should().BeTrue();

            // Act
            var success = _serviceSender.SendGift(giftItem, ReceiverName, out var giftId);
            Wait();
            var gifts = _serviceReceiver.CheckGiftBox();

            // Assert
            success.Should().BeTrue();
            gifts.Should().NotBeNull().And.HaveCount(1);
            var (receivedGiftId, receivedGift) = gifts.First();
            receivedGiftId.Should().Be(giftId);
            receivedGift.ID.Should().Be(giftId);
            receivedGift.ItemName.Should().Be(giftItem.Name);
            receivedGift.Amount.Should().Be(giftItem.Amount);
            receivedGift.ItemValue.Should().Be(giftItem.Value);
            receivedGift.SenderSlot.Should().Be(_testSessions.SenderSlot);
            receivedGift.ReceiverSlot.Should().Be(_testSessions.ReceiverSlot);
            receivedGift.SenderTeam.Should().Be(receivedGift.ReceiverTeam);
        }

        [Test]
        public void TestCannotCreateCurrentGiftForFutureGiftBoxThatIsNotBackwardCompatible()
        {
            // Arrange
            var outdatedGiftbox = new GiftBox(true)
            {
                MinimumGiftDataVersion = DataVersion.Current + 1,
                MaximumGiftDataVersion = DataVersion.Current + 1,
            };
            _serviceReceiver.UpdateGiftBox(outdatedGiftbox);
            var giftItem = NewGiftItem();
            Wait();

            // Assume
            var canGift = _serviceSender.CanGiftToPlayer(ReceiverName);
            canGift.Should().BeFalse();

            // Act
            var success = _serviceSender.SendGift(giftItem, ReceiverName, out var giftId);
            Wait();
            var gifts = _serviceReceiver.CheckGiftBox();

            // Assert
            success.Should().BeFalse();
            gifts.Should().NotBeNull().And.BeEmpty();
        }

        [Test]
        public void TestCanUnderstandGiftboxWithAllGiftVersions()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            Wait();
            var giftItems = new[] { NewGiftItem("1"), NewGiftItem("2"), NewGiftItem("3") };
            var giftIds = new string[3];
            SendVersion1Gift(giftItems[0], new GiftTrait[0], out var giftId1);
            giftIds[0] = giftId1.ToString();
            giftIds[1] = Guid.NewGuid().ToString();
            SendVersion2Gift(giftItems[1], new GiftTrait[0], giftIds[1]);
            giftIds[2] = "Unique Id that is not a Valid Guid";
            SendVersion2Gift(giftItems[2], new GiftTrait[0], giftIds[2]);
            Wait();

            // Act
            var gifts = _serviceReceiver.CheckGiftBox();

            // Assert
            gifts.Should().NotBeNull().And.HaveCount(3);
            for (var i = 0; i < 3; i++)
            {
                var giftId = giftIds[i];
                gifts.Should().ContainKey(giftId);
                var gift = gifts[giftId];
                gift.ID.Should().Be(giftId);
                gift.ItemName.Should().Be(giftItems[i].Name);
                gift.Amount.Should().Be(giftItems[i].Amount);
                gift.ItemValue.Should().Be(giftItems[i].Value);
                gift.SenderSlot.Should().Be(_testSessions.SenderSlot);
                gift.ReceiverSlot.Should().Be(_testSessions.ReceiverSlot);
                gift.SenderTeam.Should().Be(gift.ReceiverTeam);
            }
        }

        private void SendVersion1Gift(GiftItem item, GiftTrait[] traits, out Guid giftId)
        {
            var gift = new Gifts.Versions.Version1.Gift(item, traits, SenderName, ReceiverName, _testSessions.SenderTeam, _testSessions.ReceiverTeam);
            giftId = Guid.Parse(gift.ID);
            var giftboxKey = $"GiftBox;{_testSessions.ReceiverTeam};{_testSessions.ReceiverSlot}";

            var newGiftEntry = new Dictionary<Guid, Gifts.Versions.Version1.Gift>
            {
                { giftId, gift },
            };

            _sessionSender.DataStorage[Scope.Global, giftboxKey] += Operation.Update(newGiftEntry);
        }

        private void SendVersion2Gift(GiftItem item, GiftTrait[] traits, string giftId)
        {
            var gift = new Gifts.Versions.Current.Gift(item.Name, item.Amount, item.Value, traits, _testSessions.SenderSlot, _testSessions.ReceiverSlot, _testSessions.SenderTeam, _testSessions.ReceiverTeam);
            gift.ID = giftId;
            var giftboxKey = $"GiftBox;{_testSessions.ReceiverTeam};{_testSessions.ReceiverSlot}";

            var newGiftEntry = new Dictionary<string, Gifts.Versions.Current.Gift>
            {
                { gift.ID, gift },
            };

            _sessionSender.DataStorage[Scope.Global, giftboxKey] += Operation.Update(newGiftEntry);
        }
    }
}