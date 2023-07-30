using Archipelago.MultiClient.Net.Helpers;

namespace Archipelago.Gifting.Net
{
    public class GiftBox
    {
        public bool IsOpen { get; set; }
        public string Owner { get; set; }
        public string Game { get; set; }
        public bool AcceptsAnyGift { get; set; }
        public string[] DesiredTraits { get; set; }

        public GiftBox()
        {
        }

        public GiftBox(bool isOpen) : this()
        {
            IsOpen = isOpen;
        }

        public GiftBox(string owner, string game, bool acceptsAnyGift, string[] desiredTraits) : this(true)
        {
            Owner = owner;
            Game = game;
            AcceptsAnyGift = acceptsAnyGift;
            DesiredTraits = desiredTraits;
        }

        public GiftBox(string owner, string game, bool acceptsAnyGift) : this(owner, game, acceptsAnyGift, Array.Empty<string>())
        {
        }
    }
}
