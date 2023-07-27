using System.Numerics;
using Archipelago.MultiClient.Net.Helpers;

namespace Archipelago.Gifting.Net
{
    public class Gift
    {
        public Guid ID { get; set; }
        public GiftItem Item { get; set; }
        public GiftTrait[] Traits { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public bool IsRefund { get; set; }
        public BigInteger GiftValue => Item.Amount * Item.Value;

        public Gift()
        {
        }

        public Gift(GiftItem item, GiftTrait[] traits, string sender, string receiver)
        {
            ID = Guid.NewGuid();
            Item = item;
            Traits = traits;
            Sender = sender;
            Receiver = receiver;
            IsRefund = false;
        }
    }
}
