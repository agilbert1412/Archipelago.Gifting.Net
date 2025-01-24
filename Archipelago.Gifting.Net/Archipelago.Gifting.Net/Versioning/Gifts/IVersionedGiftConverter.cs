namespace Archipelago.Gifting.Net.Versioning.Gifts
{
    internal interface IVersionedGiftConverter<TCurrentGift, TPreviousGift> : IVersionedConverter<string, TCurrentGift, TPreviousGift>
    {
    }
}
