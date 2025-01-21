namespace Archipelago.Gifting.Net.Service.Result
{
    public class SuccessfulGifting : GiftingResult
    {
        public override bool Success => true;
        public override string GiftId { get; }

        public SuccessfulGifting(string giftId)
        {
            GiftId = giftId;
        }
    }
}
