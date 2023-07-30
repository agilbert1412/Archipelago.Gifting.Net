﻿using System.Numerics;
using Archipelago.MultiClient.Net.Helpers;

namespace Archipelago.Gifting.Net
{
    public class Gift
    {
        public Guid ID { get; set; }
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
            ID = Guid.NewGuid();
            Item = item;
            Traits = traits;
            SenderName = senderName;
            ReceiverName = receiverName;
            SenderTeam = senderTeam;
            ReceiverTeam = ReceiverTeam;
            IsRefund = false;
        }
    }
}
