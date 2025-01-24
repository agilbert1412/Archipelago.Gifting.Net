using Newtonsoft.Json;

namespace Archipelago.Gifting.Net.Versioning.Gifts.Current
{
    public class GiftTrait
    {
        [JsonProperty(propertyName: "trait")]
        public string Trait { get; set; }

        [JsonProperty(propertyName: "quality")]
        public double Quality { get; set; }

        [JsonProperty(propertyName: "duration")]
        public double Duration { get; set; }

        public GiftTrait() : this(null)
        {
        }

        public GiftTrait(string trait) : this(trait, 1.0)
        {
        }

        public GiftTrait(string trait, double quality) : this(trait, quality, 1.0)
        {
        }

        public GiftTrait(string trait, double quality, double duration)
        {
            Trait = trait;
            Quality = quality;
            Duration = duration;
        }
    }
}
