namespace Archipelago.Gifting.Net.Gifts.Versions.Current
{
    public class GiftTrait
    {
        public string trait { get; set; }
        public double quality { get; set; }
        public double duration { get; set; }

        public GiftTrait(string pTrait, double pDuration, double pQuality)
        {
            trait = pTrait;
            duration = pDuration;
            quality = pQuality;
        }
    }
}
