namespace Archipelago.Gifting.Net.Service.Result
{
    public class FailedGifting : GiftingResult
    {
        public override bool Success => false;
        public override string GiftId { get; }

        public string ErrorMessage { get; }

        public FailedGifting(string errorMessage) : this(string.Empty, errorMessage)
        {
        }

        public FailedGifting(string giftId, string errorMessage)
        {
            GiftId = giftId;
            ErrorMessage = errorMessage;
        }
    }
}
