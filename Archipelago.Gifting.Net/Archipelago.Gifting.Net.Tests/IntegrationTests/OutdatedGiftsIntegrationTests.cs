using Archipelago.Gifting.Net.Service;
using Archipelago.Gifting.Net.Traits;
using Archipelago.Gifting.Net.Versioning;
using Archipelago.Gifting.Net.Versioning.GiftBoxes.Current;
using Archipelago.Gifting.Net.Versioning.Gifts;
using Archipelago.Gifting.Net.Versioning.Gifts.Current;
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
            var giftId = SendVersion1Gift(giftItem, new Versioning.Gifts.Version1.GiftTrait[0]);
            WaitShort();

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
        public void TestCanReadOutdatedGiftVersion2()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            var giftItem = NewGiftItem();
            var giftId = Guid.NewGuid().ToString();
            WaitShort();
            SendVersion2Gift(giftItem, new Versioning.Gifts.Version2.GiftTrait[0], giftId);
            WaitShort();

            // Act
            var gifts = _serviceReceiver.CheckGiftBox();

            // Assert
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
        public void TestCanCreateOutdatedGiftVersion1()
        {
            // Arrange
            var outdatedGiftBox = new GiftBox(true)
            {
                MinimumGiftDataVersion = DataVersion.GIFT_DATA_VERSION_1,
                MaximumGiftDataVersion = DataVersion.GIFT_DATA_VERSION_1,
            };
            _serviceReceiver.UpdateGiftBox(outdatedGiftBox);
            var giftItem = NewGiftItem();
            WaitShort();

            // Act
            var result = _serviceSender.SendGift(giftItem, ReceiverName);
            WaitShort();

            // Assert
            result.Success.Should().BeTrue();
            var existingGiftBox = _sessionReceiver.DataStorage[Scope.Global, $"GiftBox;{_testSessions.ReceiverTeam};{_testSessions.ReceiverSlot}"];
            var gifts = new Versioning.Gifts.Version1.GiftConverter().ReadFromDataStorage(existingGiftBox);
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
            var outdatedGiftBox = new GiftBox(true)
            {
                MinimumGiftDataVersion = DataVersion.GIFT_DATA_VERSION_2,
                MaximumGiftDataVersion = DataVersion.GIFT_DATA_VERSION_2,
            };
            _serviceReceiver.UpdateGiftBox(outdatedGiftBox);
            var giftItem = NewGiftItem();
            WaitShort();

            // Act
            var result = _serviceSender.SendGift(giftItem, ReceiverName);
            WaitShort();

            // Assert
            result.Success.Should().BeTrue();
            var existingGiftBox = _sessionReceiver.DataStorage[Scope.Global, $"GiftBox;{_testSessions.ReceiverTeam};{_testSessions.ReceiverSlot}"];
            var gifts = new Versioning.Gifts.Version2.GiftConverter(_serviceReceiver.PlayerProvider).ReadFromDataStorage(existingGiftBox);
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
            canGift.CanGift.Should().BeTrue();

            // Act
            var result = _serviceSender.SendGift(giftItem, ReceiverName);
            WaitShort();
            var gifts = _serviceReceiver.CheckGiftBox();

            // Assert
            result.Success.Should().BeTrue();
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
        public void TestCannotCreateCurrentGiftForFutureGiftBoxThatIsNotBackwardCompatible()
        {
            // Arrange
            var outdatedGiftBox = new GiftBox(true)
            {
                MinimumGiftDataVersion = DataVersion.Current + 1,
                MaximumGiftDataVersion = DataVersion.Current + 1,
            };
            _serviceReceiver.UpdateGiftBox(outdatedGiftBox);
            var giftItem = NewGiftItem();
            WaitShort();

            // Assume
            var canGift = _serviceSender.CanGiftToPlayer(ReceiverName);
            canGift.CanGift.Should().BeFalse();
            canGift.Message.Should().Contain("Version 4");

            // Act
            var result = _serviceSender.SendGift(giftItem, ReceiverName);
            WaitShort();
            var gifts = _serviceReceiver.CheckGiftBox();

            // Assert
            result.Success.Should().BeFalse();
            gifts.Should().NotBeNull().And.BeEmpty();
        }

        [Test]
        public void TestCanUnderstandGiftBoxWithAllGiftVersions()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            WaitShort();
            var giftItems = new[] { NewGiftItem("1"), NewGiftItem("2"), NewGiftItem("3"), NewGiftItem("4") };
            var giftIds = new string[4];
            var giftId1 = SendVersion1Gift(giftItems[0], new Versioning.Gifts.Version1.GiftTrait[0]);
            giftIds[0] = giftId1.ToString();
            giftIds[1] = Guid.NewGuid().ToString();
            SendVersion2Gift(giftItems[1], new Versioning.Gifts.Version2.GiftTrait[0], giftIds[1]);
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
                gift.ID.Should().Be(giftId);
                gift.ItemName.Should().Be(giftItems[i].Name);
                gift.Amount.Should().Be(giftItems[i].Amount);
                gift.ItemValue.Should().Be(giftItems[i].Value);
                gift.SenderSlot.Should().Be(_testSessions.SenderSlot);
                gift.ReceiverSlot.Should().Be(_testSessions.ReceiverSlot);
                gift.SenderTeam.Should().Be(gift.ReceiverTeam);
            }
        }

        private Guid SendVersion1Gift(GiftItem item, Versioning.Gifts.Version1.GiftTrait[] traits)
        {
            var gift = new Versioning.Gifts.Version1.Gift(item, traits, SenderName, ReceiverName, _testSessions.SenderTeam, _testSessions.ReceiverTeam);
            var giftId = Guid.Parse(gift.ID);
            var giftBoxKey = $"GiftBox;{_testSessions.ReceiverTeam};{_testSessions.ReceiverSlot}";

            var newGiftEntry = new Dictionary<Guid, Versioning.Gifts.Version1.Gift>
            {
                { giftId, gift },
            };

            _sessionSender.DataStorage[Scope.Global, giftBoxKey] += Operation.Update(newGiftEntry);
            return giftId;
        }

        private void SendVersion2Gift(GiftItem item, Versioning.Gifts.Version2.GiftTrait[] traits, string giftId)
        {
            var gift = new Versioning.Gifts.Version2.Gift(item.Name, item.Amount, item.Value, traits, _testSessions.SenderSlot, _testSessions.ReceiverSlot, _testSessions.SenderTeam, _testSessions.ReceiverTeam);
            gift.ID = giftId;
            var giftBoxKey = $"GiftBox;{_testSessions.ReceiverTeam};{_testSessions.ReceiverSlot}";

            var newGiftEntry = new Dictionary<string, Versioning.Gifts.Version2.Gift>
            {
                { gift.ID, gift },
            };

            _sessionSender.DataStorage[Scope.Global, giftBoxKey] += Operation.Update(newGiftEntry);
        }

        private void SendVersion3Gift(GiftItem item, GiftTrait[] traits, string giftId)
        {
            var gift = new Gift(item.Name, item.Amount, item.Value, traits, _testSessions.SenderSlot, _testSessions.ReceiverSlot, _testSessions.SenderTeam, _testSessions.ReceiverTeam);
            gift.ID = giftId;
            var giftBoxKey = $"GiftBox;{_testSessions.ReceiverTeam};{_testSessions.ReceiverSlot}";

            var newGiftEntry = new Dictionary<string, Gift>
            {
                { gift.ID, gift },
            };

            _sessionSender.DataStorage[Scope.Global, giftBoxKey] += Operation.Update(newGiftEntry);
        }
    }
}