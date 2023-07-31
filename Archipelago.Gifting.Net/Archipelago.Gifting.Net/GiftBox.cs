using Archipelago.MultiClient.Net.Helpers;

namespace Archipelago.Gifting.Net
{
    public class GiftBox
    {
        public bool IsOpen { get; set; }
        public bool AcceptsAnyGift { get; set; }
        public string[] DesiredTraits { get; set; }

        public GiftBox()
        {
        }

        public GiftBox(bool isOpen) : this()
        {
            IsOpen = isOpen;
        }

        public GiftBox(bool acceptsAnyGift, string[] desiredTraits) : this(true)
        {
            AcceptsAnyGift = acceptsAnyGift;
            DesiredTraits = desiredTraits;
        }
    }
}
