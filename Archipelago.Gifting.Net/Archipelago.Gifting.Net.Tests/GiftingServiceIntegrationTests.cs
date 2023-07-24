using System.Collections.ObjectModel;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using FluentAssertions;
using Moq;

namespace Archipelago.Gifting.Net.Tests
{
    public class GiftingServiceIntegrationTests
    {
        private const string IP = "localhost";
        private const int PORT = 38281;
        private const string slotSender = "Sender";
        private const string slotReceiver = "Receiver";
        private const string GAME = "Stardew Valley";

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
            CloseGiftBoxes();
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
            var result = _sessionSender.TryConnectAndLogin(GAME, slotSender, itemsHandling, minimumVersion, tags);
            if (result is not LoginSuccessful)
            {
                throw new Exception($"Failed to connect as {slotSender}");
            }
        }

        private void InitializeReceiverSession(ItemsHandlingFlags itemsHandling, Version minimumVersion, string[] tags)
        {
            _sessionReceiver = ArchipelagoSessionFactory.CreateSession(IP, PORT);
            var result = _sessionReceiver.TryConnectAndLogin(GAME, slotReceiver, itemsHandling, minimumVersion, tags);
            if (result is not LoginSuccessful)
            {
                throw new Exception($"Failed to connect as {slotSender}");
            }
        }

        private void InitializeGiftingServices()
        {
            _serviceSender = new GiftingService(_sessionSender);
            _serviceReceiver = new GiftingService(_sessionReceiver);
        }

        private void CloseGiftBoxes()
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
            if (_serviceSender != null)
            {
                _sessionSender.Socket.DisconnectAsync();
                _sessionSender = null;
            }

            if (_sessionReceiver != null)
            {
                _sessionReceiver.Socket.DisconnectAsync();
                _sessionReceiver = null;
            }
        }

        [Test]
        public void TestOpenGiftboxCreatesEmptyGiftArray()
        {
            // Arrange

            // Assume
            var giftsBeforeOpeningBox = _serviceSender.CheckGiftBox();
            giftsBeforeOpeningBox.Should().BeNull();

            // Act
            _serviceSender.OpenGiftBox();

            // Assert
            var giftsAfterOpeningBox = _serviceSender.CheckGiftBox();
            giftsAfterOpeningBox.Should().NotBeNull();
            giftsAfterOpeningBox.Should().BeEmpty();
        }

        [Test]
        public void TestCloseGiftboxCreatesTurnsGiftsToNull()
        {
            // Arrange
            _serviceSender.OpenGiftBox();
            Wait();

            // Assume
            var giftsBeforeClosingBox = _serviceSender.CheckGiftBox();
            giftsBeforeClosingBox.Should().NotBeNull();
            giftsBeforeClosingBox.Should().BeEmpty();

            // Act
            _serviceSender.CloseGiftBox();

            // Assert
            var giftsAfterClosingBox = _serviceSender.CheckGiftBox();
            giftsAfterClosingBox.Should().BeNull();
        }

        [Test]
        public void TestCannotSendGiftToClosedBox()
        {
            // Arrange
            _serviceReceiver.CloseGiftBox();
            Wait();

            // Assume

            // Act
            var result = _serviceSender.CanGiftToPlayer(slotReceiver);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void TestCanSendGiftToOpenBox()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            Wait();

            // Assume

            // Act
            var result = _serviceSender.CanGiftToPlayer(slotReceiver);

            // Assert
            result.Should().BeTrue();
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
            gifts.Should().BeNull();

            // Act
            var result = _serviceSender.SendGift(gift, slotReceiver);

            // Assert
            result.Should().BeFalse();
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().BeNull();
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
            gifts.Should().NotBeNull();
            gifts.Should().HaveCount(0);

            // Act
            var result = _serviceSender.SendGift(gift, slotReceiver);
            Wait();

            // Assert
            result.Should().BeTrue();
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull();
            gifts.Should().HaveCount(1);
            gifts.Last().Item.Name.Should().Be(gift.Name);
            gifts.Last().Item.Amount.Should().Be(gift.Amount);
            gifts.Last().Item.Value.Should().Be(gift.Value);
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
            gifts.Should().NotBeNull();
            gifts.Should().HaveCount(0);

            // Act
            var result1 = _serviceSender.SendGift(gift1, slotReceiver);
            var result2 = _serviceSender.SendGift(gift2, slotReceiver);
            Wait();

            // Assert
            result1.Should().BeTrue();
            result2.Should().BeTrue();
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull();
            gifts.Should().HaveCount(2);
            gifts.First().Item.Name.Should().Be(gift1.Name);
            gifts.First().Item.Amount.Should().Be(gift1.Amount);
            gifts.First().Item.Value.Should().Be(gift1.Value);
            gifts.Last().Item.Name.Should().Be(gift2.Name);
            gifts.Last().Item.Amount.Should().Be(gift2.Amount);
            gifts.Last().Item.Value.Should().Be(gift2.Value);
        }

        [Test]
        public void TestCheckGiftsDoesNotEmptyGiftBox()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            var gift = NewGiftItem();
            Wait();
            var result = _serviceSender.SendGift(gift, slotReceiver);
            Wait();

            // Assume
            result.Should().BeTrue();

            // Act
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull();
            gifts.Should().HaveCount(1);

            // Assert
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull();
            gifts.Should().HaveCount(1);
            gifts.Last().Item.Name.Should().Be(gift.Name);
            gifts.Last().Item.Amount.Should().Be(gift.Amount);
            gifts.Last().Item.Value.Should().Be(gift.Value);
        }

        [Test]
        public void TestCheckGiftsAndEmptyEmptiesGiftBox()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            var gift = NewGiftItem();
            Wait();
            var result = _serviceSender.SendGift(gift, slotReceiver);
            Wait();

            // Assume
            result.Should().BeTrue();

            // Act
            var gifts = _serviceReceiver.GetAllGiftsAndEmptyGiftbox();

            // Assert
            gifts.Should().NotBeNull();
            gifts.Should().HaveCount(1);
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull();
            gifts.Should().BeEmpty();
        }

        [Test]
        public void TestEmptyEmptiesGiftBox()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            var gift = NewGiftItem();
            Wait();
            var result = _serviceSender.SendGift(gift, slotReceiver);
            Wait();

            // Assume
            result.Should().BeTrue();
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull();
            gifts.Should().HaveCount(1);

            // Act
            _serviceReceiver.EmptyGiftBox();
            Wait();

            // Assert
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull();
            gifts.Should().BeEmpty();
        }

        [Test]
        public void TestSendGiftWithTraitsSuceeds()
        {
            // Arrange
            _serviceReceiver.OpenGiftBox();
            var gift = NewGiftItem();
            var traits = NewGiftTraits();
            Wait();

            // Assume
            var gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull();
            gifts.Should().HaveCount(0);

            // Act
            var result = _serviceSender.SendGift(gift, traits, slotReceiver);
            Wait();

            // Assert
            result.Should().BeTrue();
            gifts = _serviceReceiver.CheckGiftBox();
            gifts.Should().NotBeNull();
            gifts.Should().HaveCount(1);
            var receivedGift = gifts.First();
            receivedGift.Item.Name.Should().Be(gift.Name);
            receivedGift.Item.Amount.Should().Be(gift.Amount);
            receivedGift.Item.Value.Should().Be(gift.Value);
            var receivedTraits = receivedGift.Traits;
            receivedTraits.Should().HaveCount(traits.Length);
            for (var i = 0; i < traits.Length; i++)
            {
                receivedTraits[i].Trait.Should().BeEquivalentTo(traits[i].Trait);
                receivedTraits[i].Strength.Should().BeApproximately(traits[i].Strength, 0.001);
                receivedTraits[i].Duration.Should().BeApproximately(traits[i].Duration, 0.001);
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
            var result = _serviceSender.SendGift(gift, traits, slotReceiver);
            Wait();

            // Assume
            result.Should().BeTrue();
            var giftsReceiver = _serviceReceiver.CheckGiftBox();
            giftsReceiver.Should().NotBeNull();
            giftsReceiver.Should().HaveCount(1);
            giftsReceiver.First().IsRefund.Should().BeFalse();
            giftsReceiver.First().Item.Name.Should().Be(gift.Name);
            giftsReceiver.First().Sender.Should().Be(slotSender);
            giftsReceiver.First().Receiver.Should().Be(slotReceiver);
            var giftsSender = _serviceSender.CheckGiftBox();
            giftsSender.Should().NotBeNull();
            giftsSender.Should().HaveCount(0);

            // Act
            giftsReceiver = _serviceReceiver.GetAllGiftsAndEmptyGiftbox();
            result = _serviceReceiver.RefundGift(giftsReceiver.Single());
            Wait();

            // Assert
            result.Should().BeTrue();
            giftsReceiver = _serviceReceiver.CheckGiftBox();
            giftsReceiver.Should().NotBeNull();
            giftsReceiver.Should().HaveCount(0);
            giftsSender = _serviceSender.CheckGiftBox();
            giftsSender.Should().NotBeNull();
            giftsSender.Should().HaveCount(1);
            giftsSender.First().IsRefund.Should().BeTrue();
            giftsSender.First().Item.Name.Should().Be(gift.Name);
            giftsSender.First().Sender.Should().Be(slotSender);
            giftsSender.First().Receiver.Should().Be(slotReceiver);
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
                var trait = new GiftTrait()
                {
                    Trait = allFlags[_random.Next(0, allFlags.Length)],
                    Duration = _random.NextDouble() * 2,
                    Strength = _random.NextDouble() * 2,
                };
                traits.Add(trait);
            }

            return traits.ToArray();
        }

        private void Wait()
        {
            Thread.Sleep(200);
        }
    }
}