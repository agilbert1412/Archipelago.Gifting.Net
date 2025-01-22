using Archipelago.Gifting.Net.Giftboxes;
using Archipelago.Gifting.Net.Gifts;
using Archipelago.Gifting.Net.Gifts.Versions;
using Archipelago.Gifting.Net.Gifts.Versions.Current;
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
        [Test]
        public void TestCanReadOutdatedGiftVersion1()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            var giftItem = NewGiftItem();
            WaitShort();
            var giftId = SendVersion1Gift(giftItem, new Gifts.Versions.Version1.GiftTrait[0]);
            WaitShort();

            // Act
            var gifts = _serviceReceiver.CheckGiftBox();

            // Assert
            gifts.Should().NotBeNull().And.HaveCount(1);
            var (receivedGiftId, receivedGift) = gifts.First();
            receivedGiftId.Should().Be(giftId.ToString());
            receivedGift.id.Should().Be(giftId.ToString());
            receivedGift.itemName.Should().Be(giftItem.Name);
            receivedGift.amount.Should().Be(giftItem.Amount);
            receivedGift.itemValue.Should().Be(giftItem.Value);
            receivedGift.senderSlot.Should().Be(_testSessions.SenderSlot);
            receivedGift.receiverSlot.Should().Be(_testSessions.ReceiverSlot);
            receivedGift.senderTeam.Should().Be(receivedGift.receiverTeam);
        }

        [Test]
        public void TestCanReadOutdatedGiftVersion2()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            var giftItem = NewGiftItem();
            var giftId = Guid.NewGuid().ToString();
            WaitShort();
            SendVersion2Gift(giftItem, new Gifts.Versions.Version2.GiftTrait[0], giftId);
            WaitShort();

            // Act
            var gifts = _serviceReceiver.CheckGiftBox();

            // Assert
            gifts.Should().NotBeNull().And.HaveCount(1);
            var (receivedGiftId, receivedGift) = gifts.First();
            receivedGiftId.Should().Be(giftId);
            receivedGift.id.Should().Be(giftId);
            receivedGift.itemName.Should().Be(giftItem.Name);
            receivedGift.amount.Should().Be(giftItem.Amount);
            receivedGift.itemValue.Should().Be(giftItem.Value);
            receivedGift.senderSlot.Should().Be(_testSessions.SenderSlot);
            receivedGift.receiverSlot.Should().Be(_testSessions.ReceiverSlot);
            receivedGift.senderTeam.Should().Be(receivedGift.receiverTeam);
        }

        [Test]
        public void TestCanCreateOutdatedGiftVersion1()
        {
            // Arrange
            var outdatedGiftbox = new GiftBox(true)
            {
                MinimumGiftDataVersion = DataVersion.GIFT_DATA_VERSION_1,
                MaximumGiftDataVersion = DataVersion.GIFT_DATA_VERSION_1,
            };
            _serviceReceiver.UpdateGiftBox(outdatedGiftbox);
            var giftItem = NewGiftItem();
            WaitShort();

            // Act
            var result = _serviceSender.SendGift(giftItem, ReceiverName);
            WaitShort();

            // Assert
            result.Success.Should().BeTrue();
            var existingGiftBox = _sessionReceiver.DataStorage[Scope.Global, $"GiftBox;{_testSessions.ReceiverTeam};{_testSessions.ReceiverSlot}"];
            var gifts = new Gifts.Versions.Version1.Converter().ReadFromDataStorage(existingGiftBox);
            gifts.Should().NotBeNull().And.HaveCount(1);
            var (receivedGiftId, receivedGift) = gifts.First();
            receivedGiftId.Should().Be(result.GiftId);
            receivedGift.ID.Should().Be(result.GiftId);
            receivedGift.Item.Should().NotBeNull();
            receivedGift.Item.Name.Should().Be(giftItem.Name);
            receivedGift.Item.Amount.Should().Be(giftItem.Amount);
            receivedGift.Item.Value.Should().Be(giftItem.Value);
            receivedGift.SenderName.Should().Be(SenderName);
            receivedGift.ReceiverName.Should().Be(ReceiverName);
            receivedGift.SenderTeam.Should().Be(receivedGift.ReceiverTeam);
        }

        [Test]
        public void TestCanCreateOutdatedGiftVersion2()
        {
            // Arrange
            var outdatedGiftbox = new GiftBox(true)
            {
                MinimumGiftDataVersion = DataVersion.GIFT_DATA_VERSION_2,
                MaximumGiftDataVersion = DataVersion.GIFT_DATA_VERSION_2,
            };
            _serviceReceiver.UpdateGiftBox(outdatedGiftbox);
            var giftItem = NewGiftItem();
            WaitShort();

            // Act
            var result = _serviceSender.SendGift(giftItem, ReceiverName);
            WaitShort();

            // Assert
            result.Success.Should().BeTrue();
            var existingGiftBox = _sessionReceiver.DataStorage[Scope.Global, $"GiftBox;{_testSessions.ReceiverTeam};{_testSessions.ReceiverSlot}"];
            var gifts = new Gifts.Versions.Version2.Converter(_serviceReceiver.PlayerProvider).ReadFromDataStorage(existingGiftBox);
            gifts.Should().NotBeNull().And.HaveCount(1);
            var (receivedGiftId, receivedGift) = gifts.First();
            receivedGiftId.Should().Be(result.GiftId);
            receivedGift.ID.Should().Be(result.GiftId);
            receivedGift.ItemName.Should().Be(giftItem.Name);
            receivedGift.Amount.Should().Be(giftItem.Amount);
            receivedGift.ItemValue.Should().Be(giftItem.Value);
            receivedGift.SenderSlot.Should().Be(_testSessions.SenderSlot);
            receivedGift.ReceiverSlot.Should().Be(_testSessions.ReceiverSlot);
            receivedGift.SenderTeam.Should().Be(receivedGift.ReceiverTeam);
        }

        [Test]
        public void TestCanCreateCurrentGiftForFutureGiftBoxThatIsBackwardCompatible()
        {
            // Arrange
            var futureGiftBox = new GiftBox(true)
            {
                MinimumGiftDataVersion = DataVersion.FirstVersion,
                MaximumGiftDataVersion = DataVersion.Current + 1,
            };
            _serviceReceiver.UpdateGiftBox(futureGiftBox);
            var giftItem = NewGiftItem();
            WaitShort();

            // Assume
            var canGift = _serviceSender.CanGiftToPlayer(ReceiverName);
            canGift.Should().BeTrue();

            // Act
            var result = _serviceSender.SendGift(giftItem, ReceiverName);
            WaitShort();
            var gifts = _serviceReceiver.CheckGiftBox();

            // Assert
            result.Success.Should().BeTrue();
            gifts.Should().NotBeNull().And.HaveCount(1);
            var (receivedGiftId, receivedGift) = gifts.First();
            receivedGiftId.Should().Be(result.GiftId);
            receivedGift.id.Should().Be(result.GiftId);
            receivedGift.itemName.Should().Be(giftItem.Name);
            receivedGift.amount.Should().Be(giftItem.Amount);
            receivedGift.itemValue.Should().Be(giftItem.Value);
            receivedGift.senderSlot.Should().Be(_testSessions.SenderSlot);
            receivedGift.receiverSlot.Should().Be(_testSessions.ReceiverSlot);
            receivedGift.senderTeam.Should().Be(receivedGift.receiverTeam);
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
            WaitShort();

            // Assume
            var canGift = _serviceSender.CanGiftToPlayer(ReceiverName);
            canGift.Should().BeFalse();

            // Act
            var result = _serviceSender.SendGift(giftItem, ReceiverName);
            WaitShort();
            var gifts = _serviceReceiver.CheckGiftBox();

            // Assert
            result.Success.Should().BeFalse();
            gifts.Should().NotBeNull().And.BeEmpty();
        }

        [Test]
        public void TestCanUnderstandGiftboxWithAllGiftVersions()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            WaitShort();
            var giftItems = new[] { NewGiftItem("1"), NewGiftItem("2"), NewGiftItem("3"), NewGiftItem("4") };
            var giftIds = new string[4];
            var giftId1 = SendVersion1Gift(giftItems[0], new Gifts.Versions.Version1.GiftTrait[0]);
            giftIds[0] = giftId1.ToString();
            giftIds[1] = Guid.NewGuid().ToString();
            SendVersion2Gift(giftItems[1], new Gifts.Versions.Version2.GiftTrait[0], giftIds[1]);
            giftIds[2] = Guid.NewGuid().ToString();
            SendVersion3Gift(giftItems[2], new GiftTrait[0], giftIds[2]);
            giftIds[3] = "Unique Id that is not a Valid Guid";
            SendVersion3Gift(giftItems[3], new GiftTrait[0], giftIds[3]);
            WaitShort();

            // Act
            var gifts = _serviceReceiver.CheckGiftBox();

            // Assert
            gifts.Should().NotBeNull().And.HaveCount(4);
            for (var i = 0; i < 4; i++)
            {
                var giftId = giftIds[i];
                gifts.Should().ContainKey(giftId);
                var gift = gifts[giftId];
                gift.id.Should().Be(giftId);
                gift.itemName.Should().Be(giftItems[i].Name);
                gift.amount.Should().Be(giftItems[i].Amount);
                gift.itemValue.Should().Be(giftItems[i].Value);
                gift.senderSlot.Should().Be(_testSessions.SenderSlot);
                gift.receiverSlot.Should().Be(_testSessions.ReceiverSlot);
                gift.senderTeam.Should().Be(gift.receiverTeam);
            }
        }

        private Guid SendVersion1Gift(GiftItem item, Gifts.Versions.Version1.GiftTrait[] traits)
        {
            var gift = new Gifts.Versions.Version1.Gift(item, traits, SenderName, ReceiverName, _testSessions.SenderTeam, _testSessions.ReceiverTeam);
            var giftId = Guid.Parse(gift.ID);
            var giftboxKey = $"GiftBox;{_testSessions.ReceiverTeam};{_testSessions.ReceiverSlot}";

            var newGiftEntry = new Dictionary<Guid, Gifts.Versions.Version1.Gift>
            {
                { giftId, gift },
            };

            _sessionSender.DataStorage[Scope.Global, giftboxKey] += Operation.Update(newGiftEntry);
            return giftId;
        }

        private void SendVersion2Gift(GiftItem item, Gifts.Versions.Version2.GiftTrait[] traits, string giftId)
        {
            var gift = new Gifts.Versions.Version2.Gift(item.Name, item.Amount, item.Value, traits, _testSessions.SenderSlot, _testSessions.ReceiverSlot, _testSessions.SenderTeam, _testSessions.ReceiverTeam);
            gift.ID = giftId;
            var giftboxKey = $"GiftBox;{_testSessions.ReceiverTeam};{_testSessions.ReceiverSlot}";

            var newGiftEntry = new Dictionary<string, Gifts.Versions.Version2.Gift>
            {
                { gift.ID, gift },
            };

            _sessionSender.DataStorage[Scope.Global, giftboxKey] += Operation.Update(newGiftEntry);
        }

        private void SendVersion3Gift(GiftItem item, Gifts.Versions.Current.GiftTrait[] traits, string giftId)
        {
            var gift = new Gifts.Versions.Current.Gift(item.Name, item.Amount, item.Value, traits, _testSessions.SenderSlot, _testSessions.ReceiverSlot, _testSessions.SenderTeam, _testSessions.ReceiverTeam);
            gift.id = giftId;
            var giftboxKey = $"GiftBox;{_testSessions.ReceiverTeam};{_testSessions.ReceiverSlot}";

            var newGiftEntry = new Dictionary<string, Gifts.Versions.Current.Gift>
            {
                { gift.id, gift },
            };

            _sessionSender.DataStorage[Scope.Global, giftboxKey] += Operation.Update(newGiftEntry);
        }
    }
}