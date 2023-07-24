using Archipelago.MultiClient.Net.Helpers;

namespace Archipelago.Gifting.Net
{
    public class GiftBox
    {
        public const string GIFTBOX_PREFIX = "GiftBox;";

        public string Owner { get; set; }

        public GiftBox(string owner)
        {
            Owner = owner;
        }

        public string GetDataStorageKey()
        {
            return $"{GIFTBOX_PREFIX}{Owner}";
        }
    }
}
