using System.Collections.Generic;
using Archipelago.Gifting.Net.Traits;
using Archipelago.Gifting.Net.Versioning.Gifts.Current;

namespace Archipelago.Gifting.Net.Utilities.CloseTraitParser
{
    public interface ICloseTraitParser<T>
    {
        void RegisterAvailableGift(T availableGift, GiftTrait[] traits);

        List<T> FindClosestAvailableGift(GiftTrait[] traits);
    }
}
