using System;
using System.Numerics;

namespace Archipelago.Gifting.Net.DTO.Version2
{
    public class Gift
    {
        public Guid ID { get; set; }
        public string ItemName { get; set; }
        public int Amount { get; set; }

        /// <summary>
        /// This is the value per item, in the standard Archipelago currency.
        /// This is not the value of the whole gift. The value of the gift is the value of the item, multiplied by the amount of the item
        /// </summary>
        public BigInteger ItemValue { get; set; }
        public GiftTrait[] Traits { get; set; }
        public string SenderName { get; set; }
        public string ReceiverName { get; set; }
        public int SenderTeam { get; set; }
        public int ReceiverTeam { get; set; }
        public bool IsRefund { get; set; }

        public Gift()
        {
        }

        public Gift(string itemName, int amount, BigInteger itemValue, GiftTrait[] traits, string senderName, string receiverName, int senderTeam, int receiverTeam)
        {
            ID = Guid.NewGuid();
            ItemName = itemName;
            Amount = amount;
            ItemValue = itemValue;
            Traits = traits;
            SenderName = senderName;
            ReceiverName = receiverName;
            SenderTeam = senderTeam;
            ReceiverTeam = ReceiverTeam;
            IsRefund = false;
        }
    }
}
