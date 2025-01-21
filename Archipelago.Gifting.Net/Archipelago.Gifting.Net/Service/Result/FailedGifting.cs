namespace Archipelago.Gifting.Net.Service.Result
{
    public class FailedGifting : GiftingResult
    {
        public override bool Success => false;
        public override string GiftId { get; }

        public FailedGifting() : this(string.Empty)
        {
        }

        public FailedGifting(string giftId)
        {
            GiftId = giftId;
        }
    }
}
