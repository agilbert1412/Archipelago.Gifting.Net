using System;
using System.Numerics;

namespace Archipelago.Gifting.Net.Gifts.Versions.Current
{
    public class Gift
    {
        public string id { get; set; }
        public string itemName { get; set; }
        public int amount { get; set; }

        /// <summary>
        /// This is the value per item, in the standard Archipelago currency.
        /// This is not the value of the whole gift. The value of the gift is the value of the item, multiplied by the amount of the item
        /// </summary>
        public BigInteger itemValue { get; set; }

        public GiftTrait[] traits { get; set; }
        public int senderSlot { get; set; }
        public int receiverSlot { get; set; }
        public int senderTeam { get; set; }
        public int receiverTeam { get; set; }
        public bool isRefund { get; set; }

        public Gift()
        {
        }

        public Gift(string itemName, int amount, BigInteger itemValue, GiftTrait[] traits, int senderSlot, int receiverSlot, int senderTeam, int receiverTeam)
        {
            id = Guid.NewGuid().ToString();
            this.itemName = itemName;
            this.amount = amount;
            this.itemValue = itemValue;
            this.traits = traits;
            this.senderSlot = senderSlot;
            this.receiverSlot = receiverSlot;
            this.senderTeam = senderTeam;
            this.receiverTeam = receiverTeam;
            isRefund = false;
        }
    }
}
