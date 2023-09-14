using System;
using System.Numerics;
using Archipelago.Gifting.Net.Traits;

namespace Archipelago.Gifting.Net.Gifts.Versions.Current
{
    public class Gift
    {
        public string ID { get; set; }
        public string ItemName { get; set; }
        public int Amount { get; set; }

        /// <summary>
        /// This is the value per item, in the standard Archipelago currency.
        /// This is not the value of the whole gift. The value of the gift is the value of the item, multiplied by the amount of the item
        /// </summary>
        public BigInteger ItemValue { get; set; }
        public GiftTrait[] Traits { get; set; }
        public int SenderSlot { get; set; }
        public int ReceiverSlot { get; set; }
        public int SenderTeam { get; set; }
        public int ReceiverTeam { get; set; }
        public bool IsRefund { get; set; }

        public Gift()
        {
        }

        public Gift(string itemName, int amount, BigInteger itemValue, GiftTrait[] traits, int senderSlot, int receiverSlot, int senderTeam, int receiverTeam)
        {
            ID = Guid.NewGuid().ToString();
            ItemName = itemName;
            Amount = amount;
            ItemValue = itemValue;
            Traits = traits;
            SenderSlot = senderSlot;
            ReceiverSlot = receiverSlot;
            SenderTeam = senderTeam;
            ReceiverTeam = receiverTeam;
            IsRefund = false;
        }
    }
}
