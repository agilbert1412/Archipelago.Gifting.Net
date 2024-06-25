using System.Collections.Generic;
using Archipelago.Gifting.Net.Traits;

namespace Archipelago.Gifting.Net.Utilities.CloseTraitParser
{
    public interface ICloseTraitParser
    {
        void RegisterAvailableGift(object availableGift, GiftTrait[] traits);

        List<object> FindClosest(GiftTrait[] traits);
    }
}
