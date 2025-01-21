namespace Archipelago.Gifting.Net.Service.Result
{
    public abstract class GiftingResult
    {
        public abstract bool Success { get; }
        public abstract string GiftId { get; }
    }
}
