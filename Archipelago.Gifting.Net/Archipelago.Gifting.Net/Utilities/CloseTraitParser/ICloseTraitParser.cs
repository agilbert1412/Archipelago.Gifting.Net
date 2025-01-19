using System.Collections.Generic;
using Archipelago.Gifting.Net.Gifts.Versions.Current;
using Archipelago.Gifting.Net.Traits;

namespace Archipelago.Gifting.Net.Utilities.CloseTraitParser
{
    public interface ICloseTraitParser<T>
    {
        void RegisterAvailableGift(T availableGift, GiftTrait[] traits);

        List<T> FindClosestAvailableGift(GiftTrait[] traits);
    }
}
