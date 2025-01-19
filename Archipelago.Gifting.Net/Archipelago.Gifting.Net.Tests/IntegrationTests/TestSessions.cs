using Archipelago.MultiClient.Net;
using Archipelago.Gifting.Net.Service;

namespace Archipelago.Gifting.Net.Tests.IntegrationTests
{
    public class TestSessions
    {
        public ArchipelagoSession? SessionSender { get; set; }
        public ArchipelagoSession? SessionReceiver { get; set; }
        public GiftingService? ServiceSender { get; set; }
        public GiftingService? ServiceReceiver { get; set; }

        public int SenderSlot => SessionSender.ConnectionInfo.Slot;
        public int ReceiverSlot => SessionReceiver.ConnectionInfo.Slot;
        public int SenderTeam => SessionSender.ConnectionInfo.Team;
        public int ReceiverTeam => SessionReceiver.ConnectionInfo.Team;

        public TestSessions()
        {

        }
    }
}
