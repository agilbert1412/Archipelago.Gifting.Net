namespace Archipelago.Gifting.Net.Versioning.GiftBoxes.Version2
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
            MaximumGiftDataVersion = DataVersion.GIFT_DATA_VERSION_2;
        }

        internal GiftBox(bool acceptsAnyGift, string[] desiredTraits) : this(true)
        {
            AcceptsAnyGift = acceptsAnyGift;
            DesiredTraits = desiredTraits;
        }
    }
}
