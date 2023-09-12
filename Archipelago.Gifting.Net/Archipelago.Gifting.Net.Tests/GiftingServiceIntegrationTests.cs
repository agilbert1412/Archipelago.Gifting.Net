using Archipelago.Gifting.Net.DTO;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using FluentAssertions;

namespace Archipelago.Gifting.Net.Tests
{
    public class GiftingServiceIntegrationTests
    {
        private const string IP = "localhost";
        private const int PORT = 38281;
        private const string SenderName = "Sender";
        private const string ReceiverName = "Receiver";
        private const string GAME = "Stardew Valley";

        private int SenderSlot => _sessionSender.ConnectionInfo.Slot;
        private int ReceiverSlot => _sessionReceiver.ConnectionInfo.Slot;
        private int SenderTeam => _sessionSender.ConnectionInfo.Team;
        private int ReceiverTeam => _sessionReceiver.ConnectionInfo.Team;

        private ArchipelagoSession? _sessionSender;
        private ArchipelagoSession? _sessionReceiver;
        private GiftingService? _serviceSender;
        private GiftingService? _serviceReceiver;

        private Random _random = new Random(1234);

        [SetUp]
        public void Setup()
        {
            Wait();
            InitializeSessions();
            Wait();
            InitializeGiftingServices();
            Wait();
        }

        [TearDown]
        public void TearDown()
        {
            Wait();
            CloseGiftBoxesAndShutdownGiftingServices();
            Wait();
            DisconnectSessions();
            Wait();
        }

        private void InitializeSessions()
        {
            var itemsHandling = ItemsHandlingFlags.AllItems;
            var minimumVersion = new Version(0, 4, 2);
            var tags = new[] { "AP" };
            InitializeSenderSession(itemsHandling, minimumVersion, tags);
            InitializeReceiverSession(itemsHandling, minimumVersion, tags);
        }

        private void InitializeSenderSession(ItemsHandlingFlags itemsHandling, Version minimumVersion, string[] tags)
        {
            _sessionSender = ArchipelagoSessionFactory.CreateSession(IP, PORT);
            var result = _sessionSender.TryConnectAndLogin(GAME, SenderName, itemsHandling, minimumVersion, tags);
            if (result is not LoginSuccessful)
            {
                throw new Exception($"Failed to connect as {SenderName}");
            }
        }

        private void InitializeReceiverSession(ItemsHandlingFlags itemsHandling, Version minimumVersion, string[] tags)
        {
            _sessionReceiver = ArchipelagoSessionFactory.CreateSession(IP, PORT);
            var result = _sessionReceiver.TryConnectAndLogin(GAME, ReceiverName, itemsHandling, minimumVersion, tags);
            if (result is not LoginSuccessful)
            {
                throw new Exception($"Failed to connect as {SenderName}");
            }
        }

        private void InitializeGiftingServices()
        {
            _serviceSender = new GiftingService(_sessionSender);
            _serviceReceiver = new GiftingService(_sessionReceiver);
            _serviceSender.CloseGiftBox();
            _serviceReceiver.CloseGiftBox();
        }

        private void CloseGiftBoxesAndShutdownGiftingServices()
        {
            if (_serviceSender != null)
            {
                _serviceSender.CloseGiftBox();
                _serviceSender = null;
            }

            if (_serviceReceiver != null)
            {
                _serviceReceiver.CloseGiftBox();
                _serviceReceiver = null;
            }
        }

        private void DisconnectSessions()
        {
            if (_sessionSender != null)
            {
                RemoveAlias(_sessionSender);
                _sessionSender.Socket.DisconnectAsync();
                _sessionSender = null;
            }

            if (_sessionReceiver != null)
            {
                RemoveAlias(_sessionReceiver);
                _sessionReceiver.Socket.DisconnectAsync();
                _sessionReceiver = null;
            }
        }

        [Test]
        public void TestCannotSendGiftToNeverOpenedBox()
        {
            // Arrange
            _serviceReceiver.CloseGiftBox();
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
            result.Should().BeFalse();
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
            var result = _serviceSender.SendGift(gift, ReceiverName, out var giftId);
            Wait();

            // Assert
            result.Should().BeTrue();
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(1);
            var (receivedGiftId, receivedGift) = gifts.First();
            receivedGiftId.Should().Be(giftId);
            receivedGift.ID.Should().Be(giftId);
            receivedGift.ItemName.Should().Be(gift.Name);
            receivedGift.Amount.Should().Be(gift.Amount);
            receivedGift.ItemValue.Should().Be(gift.Value);
            receivedGift.SenderSlot.Should().Be(SenderSlot);
            receivedGift.ReceiverSlot.Should().Be(ReceiverSlot);
            receivedGift.SenderTeam.Should().Be(receivedGift.ReceiverTeam);
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
            var result1 = _serviceSender.SendGift(gift1, ReceiverName, out var giftId1);
            var result2 = _serviceSender.SendGift(gift2, ReceiverName, out var giftId2);
            Wait();

            // Assert
            result1.Should().BeTrue();
            result2.Should().BeTrue();
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(2);
            var receivedGift1 = gifts[giftId1];
            var receivedGift2 = gifts[giftId2];
            receivedGift1.ItemName.Should().Be(gift1.Name);
            receivedGift1.Amount.Should().Be(gift1.Amount);
            receivedGift1.ItemValue.Should().Be(gift1.Value);
            receivedGift1.SenderSlot.Should().Be(SenderSlot);
            receivedGift1.ReceiverSlot.Should().Be(ReceiverSlot);
            receivedGift1.SenderTeam.Should().Be(receivedGift1.ReceiverTeam);
            receivedGift2.ItemName.Should().Be(gift2.Name);
            receivedGift2.Amount.Should().Be(gift2.Amount);
            receivedGift2.ItemValue.Should().Be(gift2.Value);
            receivedGift2.SenderSlot.Should().Be(SenderSlot);
            receivedGift2.ReceiverSlot.Should().Be(ReceiverSlot);
            receivedGift2.SenderTeam.Should().Be(receivedGift2.ReceiverTeam);
        }

        [Test]
        public void TestCheckGiftsDoesNotEmptyGiftBox()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            var gift = NewGiftItem();
            Wait();
            var result = _serviceSender.SendGift(gift, ReceiverName, out var giftId);
            Wait();

            // Assume
            result.Should().BeTrue();

            // Act
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(1);

            // Assert
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(1);
            var (receivedGiftId, receivedGift) = gifts.First();
            receivedGiftId.Should().Be(giftId);
            receivedGift.ID.Should().Be(giftId);
            receivedGift.ItemName.Should().Be(gift.Name);
            receivedGift.Amount.Should().Be(gift.Amount);
            receivedGift.ItemValue.Should().Be(gift.Value);
            receivedGift.SenderSlot.Should().Be(SenderSlot);
            receivedGift.ReceiverSlot.Should().Be(ReceiverSlot);
            receivedGift.SenderTeam.Should().Be(receivedGift.ReceiverTeam);
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
            result.Should().BeTrue();

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
            var result1 = _serviceSender.SendGift(gift1, ReceiverName, out var giftId1);
            var result2 = _serviceSender.SendGift(gift2, ReceiverName, out var giftId2);
            Wait();

            // Assume
            result1.Should().BeTrue();
            result2.Should().BeTrue();
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(2);

            // Act
            _serviceReceiver.RemoveGiftFromGiftBox(giftId2);
            Wait();

            // Assert
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(1);
            gifts.Should().ContainKey(giftId1);
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
            var result1 = _serviceSender.SendGift(gift1, ReceiverName, out var giftId1);
            var result2 = _serviceSender.SendGift(gift2, ReceiverName, out var giftId2);
            var result3 = _serviceSender.SendGift(gift3, ReceiverName, out var giftId3);
            var result4 = _serviceSender.SendGift(gift4, ReceiverName, out var giftId4);
            Wait();

            // Assume
            result1.Should().BeTrue();
            result2.Should().BeTrue();
            result3.Should().BeTrue();
            result4.Should().BeTrue();
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(4);

            // Act
            _serviceReceiver.RemoveGiftsFromGiftBox(new[] {giftId1, giftId4});
            Wait();

            // Assert
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(2);
            gifts.Should().ContainKey(giftId2);
            gifts.Should().ContainKey(giftId3);
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
            var result = _serviceSender.SendGift(gift, traits, ReceiverName, out var giftId);
            Wait();

            // Assert
            result.Should().BeTrue();
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(1);
            var receivedGift = gifts[giftId];
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
            Wait();

            // Assume
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull();
            gifts.Should().HaveCount(0);

            // Act
            var result1 = _serviceSender.SendGift(gift1, traits1, ReceiverName, out var giftId1);
            var result2 = _serviceSender.SendGift(gift2, traits2, ReceiverName, out var giftId2);
            Wait();

            // Assert
            result1.Should().BeTrue();
            result2.Should().BeTrue();
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull();
            gifts.Should().HaveCount(2);
            var receivedGift1 = gifts[giftId1];
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
            var receivedGift2 = gifts[giftId2];
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
            Wait();
            var result = _serviceSender.SendGift(gift, traits, ReceiverName, out var giftId);
            Wait();

            // Assume
            result.Should().BeTrue();
            var giftsReceiver = _serviceReceiver.CheckGiftBox();
            giftsReceiver.Should().NotBeNull();
            giftsReceiver.Should().HaveCount(1);
            var giftReceiver = giftsReceiver[giftId];
            giftReceiver.IsRefund.Should().BeFalse();
            giftReceiver.ItemName.Should().Be(gift.Name);
            giftReceiver.SenderSlot.Should().Be(SenderSlot);
            giftReceiver.ReceiverSlot.Should().Be(ReceiverSlot);
            giftReceiver.SenderTeam.Should().Be(giftReceiver.ReceiverTeam);
            var giftsSender = _serviceSender.CheckGiftBox();
            giftsSender.Should().BeEmpty();

            // Act
            giftsReceiver = _serviceReceiver.GetAllGiftsAndEmptyGiftbox();
            result = _serviceReceiver.RefundGift(giftsReceiver[giftId]);
            Wait();

            // Assert
            result.Should().BeTrue();
            giftsReceiver = _serviceReceiver.CheckGiftBox();
            giftsReceiver.Should().BeEmpty();
            giftsSender = _serviceSender.CheckGiftBox();
            giftsSender.Should().NotBeNull().And.HaveCount(1);
            var giftSender = giftsSender[giftId];
            giftSender.IsRefund.Should().BeTrue();
            giftSender.ItemName.Should().Be(gift.Name);
            giftSender.ItemName.Should().Be(gift.Name);
            giftSender.SenderSlot.Should().Be(SenderSlot);
            giftSender.ReceiverSlot.Should().Be(ReceiverSlot);
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
            var result = _serviceSender.SendGift(gift, "receiver2", out var giftId);
            Wait();

            // Assert
            result.Should().BeTrue();
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(1);
            var (receivedGiftId, receivedGift) = gifts.First();
            receivedGiftId.Should().Be(giftId);
            receivedGift.ID.Should().Be(giftId);
            receivedGift.ItemName.Should().Be(gift.Name);
            receivedGift.Amount.Should().Be(gift.Amount);
            receivedGift.ItemValue.Should().Be(gift.Value);
            receivedGift.SenderSlot.Should().Be(SenderSlot);
            receivedGift.ReceiverSlot.Should().Be(ReceiverSlot);
            receivedGift.SenderTeam.Should().Be(receivedGift.ReceiverTeam);
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
            var result = _serviceSender.SendGift(gift, ReceiverName, out var giftId);
            Wait();

            // Assert
            result.Should().BeTrue();
            var giftsSender = _serviceSender.CheckGiftBox();
            giftsSender.Should().BeEmpty();
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull().And.HaveCount(1);
            var (receivedGiftId, receivedGift) = gifts.First();
            receivedGiftId.Should().Be(giftId);
            receivedGift.ID.Should().Be(giftId);
            receivedGift.ItemName.Should().Be(gift.Name);
            receivedGift.Amount.Should().Be(gift.Amount);
            receivedGift.ItemValue.Should().Be(gift.Value);
            receivedGift.SenderSlot.Should().Be(SenderSlot);
            receivedGift.ReceiverSlot.Should().Be(ReceiverSlot);
            receivedGift.SenderTeam.Should().Be(receivedGift.ReceiverTeam);
        }

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
            receivedGiftId.Should().Be(giftId);
            receivedGift.ID.Should().Be(giftId);
            receivedGift.ItemName.Should().Be(giftItem.Name);
            receivedGift.Amount.Should().Be(giftItem.Amount);
            receivedGift.ItemValue.Should().Be(giftItem.Value);
            receivedGift.SenderSlot.Should().Be(SenderSlot);
            receivedGift.ReceiverSlot.Should().Be(ReceiverSlot);
            receivedGift.SenderTeam.Should().Be(receivedGift.ReceiverTeam);
        }

        [Test]
        public void TestCanCreateOutdatedGift()
        {
            // Arrange
            var outdatedGiftbox = new GiftBox(true)
            {
                GiftDataVersion = DataVersion.GiftDataVersion1,
            };
            _serviceReceiver.UpdateGiftBox(outdatedGiftbox);
            var giftItem = NewGiftItem();
            Wait();

            // Act
            var success = _serviceSender.SendGift(giftItem, ReceiverName, out var giftId);
            Wait();

            // Assert
            success.Should().BeTrue();
            var existingGiftBox = _sessionReceiver.DataStorage[Scope.Global, $"GiftBox;{ReceiverTeam};{ReceiverSlot}"];
            var gifts = new DTO.Version1.Converter().ReadFromDataStorage(existingGiftBox);
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

        private GiftItem NewGiftItem()
        {
            return new GiftItem("Test Gift", _random.Next(1, 10), _random.Next(1, 100));
        }

        private GiftTrait[] NewGiftTraits()
        {
            var count = _random.Next(0, 5);
            var allFlags = GiftFlag.AllFlags;
            var traits = new List<GiftTrait>();
            for (var i = 0; i < count; i++)
            {
                var trait = new GiftTrait(trait: allFlags[_random.Next(0, allFlags.Length)],
                    duration: _random.NextDouble() * 2, quality: _random.NextDouble() * 2);
                traits.Add(trait);
            }

            return traits.ToArray();
        }

        private void Wait()
        {
            Thread.Sleep(50);
        }

        private void SetAlias(ArchipelagoSession session, string alias)
        {

            var packet = new SayPacket()
            {
                Text = $"!alias {alias}",
            };

            session.Socket.SendPacket(packet);
            Wait();
        }

        private void RemoveAlias(ArchipelagoSession session)
        {
            var packet = new SayPacket()
            {
                Text = $"!alias",
            };

            session.Socket.SendPacket(packet);
            Wait();
        }

        private void SendVersion1Gift(GiftItem item, GiftTrait[] traits, out Guid giftId)
        {
            var gift = new DTO.Version1.Gift(item, traits, SenderName, ReceiverName, SenderTeam, ReceiverTeam);
            giftId = gift.ID;
            var giftboxKey = $"GiftBox;{ReceiverTeam};{ReceiverSlot}";
            
            var newGiftEntry = new Dictionary<Guid, DTO.Version1.Gift>
            {
                { gift.ID, gift },
            };

            _sessionSender.DataStorage[Scope.Global, giftboxKey] += Operation.Update(newGiftEntry);
        }
    }
}