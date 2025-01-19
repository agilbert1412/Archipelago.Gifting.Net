using System.Collections.ObjectModel;
using Archipelago.Gifting.Net.Service;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Helpers;
using FluentAssertions;
using Moq;

namespace Archipelago.Gifting.Net.Tests.UnitTests
{
    [Ignore("Cannot mock things the way I need to")]
    public class GiftingServiceUnitTests
    {
        private Mock<ArchipelagoSession> _senderSession;
        private Mock<ArchipelagoSession> _receiverSession;
        private Mock<PlayerHelper> _playerHelper;
        private Mock<PlayerInfo> _senderPlayer;
        private Mock<PlayerInfo> _receiverPlayer;

        private GiftingService _service;

        [SetUp]
        public void Setup()
        {
            InitializePlayerHelper();
            InitializeSenderSession();
            InitializeReceiverSession();
        }

        [Test]
        public void TestOpenGiftboxCreatesEmptyGiftArray()
        {
            // Arrange
            _service = new GiftingService(_senderSession.Object);

            // Assume
            var giftsBeforeOpeningBox = _service.CheckGiftBox();
            giftsBeforeOpeningBox.Should().BeNull();

            // Act
            _service.OpenGiftBox();

            // Assert
            var giftsAfterOpeningBox = _service.CheckGiftBox();
            giftsAfterOpeningBox.Should().NotBeNull();
            giftsAfterOpeningBox.Should().BeEmpty();
        }

        private void InitializePlayerHelper()
        {
            _playerHelper = new Mock<PlayerHelper>();

            InitializeSenderPlayer();
            InitializeReceiverPlayer();
            var playersByTeam = new Dictionary<int, ReadOnlyCollection<PlayerInfo>>();
            var players = new List<PlayerInfo> { _senderPlayer.Object, _receiverPlayer.Object };
            var readonlyPlayers = new ReadOnlyCollection<PlayerInfo>(players);
            playersByTeam.Add(0, readonlyPlayers);
            var readonlyPlayersByTeam = new ReadOnlyDictionary<int, ReadOnlyCollection<PlayerInfo>>(playersByTeam);
            _playerHelper.Setup(x => x.Players).Returns(readonlyPlayersByTeam);
        }

        private void InitializeSenderPlayer()
        {
            _senderPlayer = new Mock<PlayerInfo>();
            _senderPlayer.SetupGet(x => x.Name).Returns("Sender");
            _senderPlayer.SetupGet(x => x.Alias).Returns("AliasSender");
            _senderPlayer.SetupGet(x => x.Slot).Returns(0);
        }

        private void InitializeReceiverPlayer()
        {
            _receiverPlayer = new Mock<PlayerInfo>();
            _receiverPlayer.SetupGet(x => x.Name).Returns("Receiver");
            _receiverPlayer.SetupGet(x => x.Alias).Returns("AliasReceiver");
            _receiverPlayer.SetupGet(x => x.Slot).Returns(1);
        }

        private void InitializeSenderSession()
        {
            _senderSession = new Mock<ArchipelagoSession>();
            _senderSession.Setup(x => x.Players).Returns(_playerHelper.Object);
            _senderSession.Setup(x => x.ConnectionInfo.Slot).Returns(0);
        }

        private void InitializeReceiverSession()
        {
            _senderSession = new Mock<ArchipelagoSession>();
            _senderSession.Setup(x => x.Players).Returns(_playerHelper.Object);
            _senderSession.Setup(x => x.ConnectionInfo.Slot).Returns(1);
        }
    }
}