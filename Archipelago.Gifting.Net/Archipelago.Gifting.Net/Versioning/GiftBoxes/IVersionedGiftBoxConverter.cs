namespace Archipelago.Gifting.Net.Versioning.GiftBoxes
{
    internal interface IVersionedGiftBoxConverter<TCurrentGiftBox, TPreviousGiftBox> : IVersionedConverter<int, TCurrentGiftBox, TPreviousGiftBox>
    {
    }
}
