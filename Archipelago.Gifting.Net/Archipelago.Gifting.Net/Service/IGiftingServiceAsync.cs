using System.Collections.Generic;
using System.Threading.Tasks;
using Archipelago.Gifting.Net.Gifts;
using Archipelago.Gifting.Net.Gifts.Versions.Current;
using Archipelago.Gifting.Net.Traits;

namespace Archipelago.Gifting.Net.Service
{
    public interface IGiftingServiceAsync
    {
        Task<Dictionary<string, Gift>> CheckGiftBoxAsync();
        Task<bool> CanGiftToPlayerAsync(int playerSlot, int playerTeam, IEnumerable<string> giftTraits);
        Task<bool> SendGiftAsync(GiftItem item, GiftTrait[] traits, string playerName, int playerTeam);
    }
}
