using Archipelago.Gifting.Net.Gifts.Versions;

namespace Archipelago.Gifting.Net.Giftboxes
{
    public class GiftBox
    {
        public bool isOpen { get; set; }
        public bool acceptsAnyGift { get; set; }
        public string[] desiredTraits { get; set; }
        public int minimumGiftDataVersion { get; set; }
        public int maximumGiftDataVersion { get; set; }

        internal GiftBox()
        {
        }

        internal GiftBox(bool isOpen) : this()
        {
            this.isOpen = isOpen;
            acceptsAnyGift = true;
            desiredTraits = new string[0];
            minimumGiftDataVersion = DataVersion.FirstVersion;
            maximumGiftDataVersion = DataVersion.Current;
        }

        internal GiftBox(bool acceptsAnyGift, string[] desiredTraits) : this(true)
        {
            this.acceptsAnyGift = acceptsAnyGift;
            this.desiredTraits = desiredTraits;
        }
    }
}
