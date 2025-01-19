namespace Archipelago.Gifting.Net.Gifts.Versions.Current
{
    public class GiftTrait
    {
        public string trait { get; set; }
        public double quality { get; set; }
        public double duration { get; set; }

        public GiftTrait() : this(null)
        {
        }

        public GiftTrait(string pTrait) : this(pTrait, 1.0)
        {
        }

        public GiftTrait(string pTrait, double pQuality) : this(pTrait, pQuality, 1.0)
        {
        }

        public GiftTrait(string pTrait, double pQuality, double pDuration)
        {
            trait = pTrait;
            duration = pDuration;
            quality = pQuality;
        }
    }
}
