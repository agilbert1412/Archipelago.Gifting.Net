using Archipelago.Gifting.Net.Gifts;
using Archipelago.Gifting.Net.Gifts.Versions.Current;
using Archipelago.Gifting.Net.Service;
using Archipelago.Gifting.Net.Traits;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Packets;

namespace Archipelago.Gifting.Net.Tests.IntegrationTests
{
    public class IntegrationTestBase
    {
        protected const string IP = "localhost";
        protected const int PORT = 38281;
        protected const string SenderName = "Sender";
        protected const string ReceiverName = "Receiver";
        protected const string GAME = "Clique";

        protected Random _random = new Random(1234);

        protected TestSessions _testSessions = new();

        protected ArchipelagoSession? _sessionSender => _testSessions.SessionSender;

        protected ArchipelagoSession? _sessionReceiver => _testSessions.SessionReceiver;

        protected GiftingService? _serviceSender => _testSessions.ServiceSender;

        protected GiftingService? _serviceReceiver => _testSessions.ServiceReceiver;

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

        protected void InitializeSessions()
        {
            var itemsHandling = ItemsHandlingFlags.AllItems;
            var minimumVersion = new Version(0, 4, 2);
            var tags = new[] { "AP" };
            InitializeSenderSession(itemsHandling, minimumVersion, tags);
            InitializeReceiverSession(itemsHandling, minimumVersion, tags);
        }

        protected void InitializeSenderSession(ItemsHandlingFlags itemsHandling, Version minimumVersion, string[] tags)
        {
            _testSessions.SessionSender = ArchipelagoSessionFactory.CreateSession(IP, PORT);
            var result = _testSessions.SessionSender.TryConnectAndLogin(GAME, SenderName, itemsHandling, minimumVersion, tags);
            if (result is not LoginSuccessful)
            {
                throw new Exception($"Failed to connect as {SenderName}");
            }
        }

        protected void InitializeReceiverSession(ItemsHandlingFlags itemsHandling, Version minimumVersion, string[] tags)
        {
            _testSessions.SessionReceiver = ArchipelagoSessionFactory.CreateSession(IP, PORT);
            var result = _testSessions.SessionReceiver.TryConnectAndLogin(GAME, ReceiverName, itemsHandling, minimumVersion, tags);
            if (result is not LoginSuccessful)
            {
                throw new Exception($"Failed to connect as {SenderName}");
            }
        }

        protected void InitializeGiftingServices()
        {
            _testSessions.ServiceSender = new GiftingService(_testSessions.SessionSender);
            _testSessions.ServiceReceiver = new GiftingService(_testSessions.SessionReceiver);
            _testSessions.ServiceSender.CloseGiftBox();
            _testSessions.ServiceReceiver.CloseGiftBox();
        }

        protected void CloseGiftBoxesAndShutdownGiftingServices()
        {
            if (_testSessions.ServiceSender != null)
            {
                _testSessions.ServiceSender.CloseGiftBox();
                _testSessions.ServiceSender = null;
            }

            if (_testSessions.ServiceReceiver != null)
            {
                _testSessions.ServiceReceiver.CloseGiftBox();
                _testSessions.ServiceReceiver = null;
            }
        }

        protected void DisconnectSessions()
        {
            if (_testSessions.SessionSender != null)
            {
                RemoveAlias(_testSessions.SessionSender);
                _testSessions.SessionSender.Socket.DisconnectAsync();
                _testSessions.SessionSender = null;
            }

            if (_testSessions.SessionReceiver != null)
            {
                RemoveAlias(_testSessions.SessionReceiver);
                _testSessions.SessionReceiver.Socket.DisconnectAsync();
                _testSessions.SessionReceiver = null;
            }
        }

        protected void Wait(int ms = 50)
        {
            Thread.Sleep(ms);
        }

        protected GiftItem NewGiftItem(string suffix = "")
        {
            return new GiftItem("Test Gift" + (string.IsNullOrEmpty(suffix) ? "" : $" {suffix}"), _random.Next(1, 10), _random.Next(1, 100));
        }

        protected GiftTrait[] NewGiftTraits()
        {
            var count = _random.Next(0, 5);
            var allFlags = GiftFlag.AllFlags;
            var traits = new List<GiftTrait>();
            for (var i = 0; i < count; i++)
            {
                var trait = new GiftTrait(allFlags[_random.Next(0, allFlags.Length)],
                    _random.NextDouble() * 2, _random.NextDouble() * 2);
                traits.Add(trait);
            }

            return traits.ToArray();
        }

        protected void SetAlias(ArchipelagoSession session, string alias)
        {

            var packet = new SayPacket()
            {
                Text = $"!alias {alias}",
            };

            session.Socket.SendPacket(packet);
            Wait();
        }

        protected void RemoveAlias(ArchipelagoSession session)
        {
            var packet = new SayPacket()
            {
                Text = $"!alias",
            };

            session.Socket.SendPacket(packet);
            Wait();
        }

        protected void CloseReceiverGiftBox()
        {
            _testSessions.ServiceReceiver.CloseGiftBox();
        }
    }
}
