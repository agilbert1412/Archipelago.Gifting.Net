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
            WaitMedium();
            InitializeSessions();
            WaitShort();
            InitializeGiftingServices();
            WaitShort();
        }

        [TearDown]
        public void TearDown()
        {
            WaitShort();
            CloseGiftBoxesAndShutdownGiftingServices();
            WaitShort();
            DisconnectSessions();
            WaitMedium();
        }

        protected void InitializeSessions()
        {
            var itemsHandling = ItemsHandlingFlags.AllItems;
            var minimumVersion = new Version(0, 5, 1);
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
                throw new Exception($"Failed to connect as {SenderName}. {result}");
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

        protected void WaitShort()
        {
            Wait(50);
        }

        protected async Task WaitShortAsync()
        {
            await WaitAsync(50);
        }

        protected void WaitMedium()
        {
            Wait(200);
        }

        protected async Task WaitMediumAsync()
        {
            await WaitAsync(200);
        }

        protected void WaitLong()
        {
            Wait(1000);
        }

        protected async Task WaitLongAsync()
        {
            await WaitAsync(1000);
        }

        private void Wait(int ms)
        {
            Thread.Sleep(ms);
        }

        private async Task WaitAsync(int ms)
        {
            await Task.Delay(ms);
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
            WaitShort();
        }

        protected void RemoveAlias(ArchipelagoSession session)
        {
            var packet = new SayPacket()
            {
                Text = $"!alias",
            };

            session.Socket.SendPacket(packet);
            WaitShort();
        }

        protected void CloseReceiverGiftBox()
        {
            _testSessions.ServiceReceiver.CloseGiftBox();
        }
    }
}
