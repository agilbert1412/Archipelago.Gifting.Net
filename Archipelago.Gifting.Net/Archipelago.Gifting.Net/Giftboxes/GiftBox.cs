using Archipelago.Gifting.Net.Gifts.Versions;

namespace Archipelago.Gifting.Net.Giftboxes
{
    public class GiftBox
    {
        public bool IsOpen { get; set; }
        public bool AcceptsAnyGift { get; set; }
        public string[] DesiredTraits { get; set; }
        public int MinimumGiftDataVersion { get; set; }
        public int MaximumGiftDataVersion { get; set; }

        internal GiftBox()
        {
        }

        internal GiftBox(bool isOpen) : this()
        {
            IsOpen = isOpen;
            AcceptsAnyGift = true;
            DesiredTraits = new string[0];
            MinimumGiftDataVersion = DataVersion.FirstVersion;
            MaximumGiftDataVersion = DataVersion.Current;
        }

        internal GiftBox(bool acceptsAnyGift, string[] desiredTraits) : this(true)
        {
            AcceptsAnyGift = acceptsAnyGift;
            DesiredTraits = desiredTraits;
        }
    }
}
