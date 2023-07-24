using System.Numerics;
using Archipelago.MultiClient.Net.Helpers;

namespace Archipelago.Gifting.Net
{
    public class Gift
    {
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
            Item = item;
            Traits = traits;
            Sender = sender;
            Receiver = receiver;
            IsRefund = false;
        }

        public Gift(GiftItem item, GiftTrait[] traits, string sender, string receiver, bool isRefund)
        {
            Item = item;
            Traits = traits;
            Sender = sender;
            Receiver = receiver;
            IsRefund = isRefund;
        }
    }
}
