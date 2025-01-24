using System;
using System.Numerics;

namespace Archipelago.Gifting.Net.Versioning.Gifts.Version1
{
    internal class Gift
    {
        public string ID { get; set; }
        public GiftItem Item { get; set; }
        public GiftTrait[] Traits { get; set; }
        public string SenderName { get; set; }
        public string ReceiverName { get; set; }
        public int SenderTeam { get; set; }
        public int ReceiverTeam { get; set; }
        public bool IsRefund { get; set; }
        public BigInteger GiftValue => Item.Amount * Item.Value;

        public Gift()
        {
        }

        public Gift(GiftItem item, GiftTrait[] traits, string senderName, string receiverName, int senderTeam, int receiverTeam)
        {
            ID = Guid.NewGuid().ToString();
            Item = item;
            Traits = traits;
            SenderName = senderName;
            ReceiverName = receiverName;
            SenderTeam = senderTeam;
            ReceiverTeam = receiverTeam;
            IsRefund = false;
        }
    }
}
