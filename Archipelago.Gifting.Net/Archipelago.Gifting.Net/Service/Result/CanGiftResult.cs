namespace Archipelago.Gifting.Net.Service.Result
{
    public class CanGiftResult
    {
        public bool CanGift => string.IsNullOrWhiteSpace(Message);
        public string Message { get; }

        public CanGiftResult() : this(string.Empty)
        {
        }

        public CanGiftResult(string message)
        {
            Message = message;
        }
    }
}
