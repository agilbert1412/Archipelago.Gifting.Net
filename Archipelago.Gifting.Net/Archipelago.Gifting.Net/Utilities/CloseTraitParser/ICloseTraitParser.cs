using System.Collections.Generic;
using Archipelago.Gifting.Net.Gifts;
using Archipelago.Gifting.Net.Traits;

namespace Archipelago.Gifting.Net.Utilities.CloseTraitParser
{
    public interface ICloseTraitParser
    {
        void RegisterGiftItem(GiftItem giftItem, GiftTrait[] traits);

        List<GiftItem> FindClosest(GiftTrait[] traits);
    }
}
