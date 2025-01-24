using System;
using System.Numerics;
using Newtonsoft.Json;

namespace Archipelago.Gifting.Net.Versioning.Gifts.Current
{
    public class Gift
    {
        [JsonProperty(propertyName: "id")]
        public string ID { get; set; }

        [JsonProperty(propertyName: "item_name")]
        public string ItemName { get; set; }

        [JsonProperty(propertyName: "amount")]
        public int Amount { get; set; }

        /// <summary>
        /// This is the value per item, in the standard Archipelago currency.
        /// This is not the value of the whole gift. The value of the gift is the value of the item, multiplied by the amount of the item
        /// </summary>
        [JsonProperty(propertyName: "item_value")]
        public BigInteger ItemValue { get; set; }

        [JsonProperty(propertyName: "traits")]
        public GiftTrait[] Traits { get; set; }

        [JsonProperty(propertyName: "sender_slot")]
        public int SenderSlot { get; set; }

        [JsonProperty(propertyName: "receiver_slot")]
        public int ReceiverSlot { get; set; }

        [JsonProperty(propertyName: "sender_team")]
        public int SenderTeam { get; set; }

        [JsonProperty(propertyName: "receiver_team")]
        public int ReceiverTeam { get; set; }

        [JsonProperty(propertyName: "is_refund")]
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
