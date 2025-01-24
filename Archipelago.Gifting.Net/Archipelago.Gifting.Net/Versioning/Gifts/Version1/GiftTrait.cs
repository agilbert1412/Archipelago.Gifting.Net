namespace Archipelago.Gifting.Net.Versioning.Gifts.Version1
{
    public class GiftTrait
    {
        public string Trait { get; set; }
        public double Quality { get; set; }
        public double Duration { get; set; }

        public GiftTrait(string trait, double duration, double quality)
        {
            Trait = trait;
            Quality = quality;
            Duration = duration;
        }
    }
}
