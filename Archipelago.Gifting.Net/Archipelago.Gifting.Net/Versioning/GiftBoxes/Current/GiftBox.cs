using Newtonsoft.Json;

namespace Archipelago.Gifting.Net.Versioning.GiftBoxes.Current
{
    public class GiftBox
    {
        [JsonProperty(propertyName: "is_open")]
        public bool IsOpen { get; set; }

        [JsonProperty(propertyName: "accepts_any_gift")]
        public bool AcceptsAnyGift { get; set; }

        [JsonProperty(propertyName: "desired_traits")]
        public string[] DesiredTraits { get; set; }

        [JsonProperty(propertyName: "minimum_gift_data_version")]
        public int MinimumGiftDataVersion { get; set; }

        [JsonProperty(propertyName: "maximum_gift_data_version")]
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
