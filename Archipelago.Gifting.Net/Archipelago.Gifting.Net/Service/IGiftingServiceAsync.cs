using System.Collections.Generic;
using System.Threading.Tasks;
using Archipelago.Gifting.Net.Service.Result;
using Archipelago.Gifting.Net.Versioning.Gifts;
using Archipelago.Gifting.Net.Versioning.Gifts.Current;

namespace Archipelago.Gifting.Net.Service
{
    public interface IGiftingServiceAsync
    {
        Task<Dictionary<string, Gift>> CheckGiftBoxAsync();
        Task<bool> CanGiftToPlayerAsync(int playerSlot, int playerTeam, IEnumerable<string> giftTraits);
        Task<GiftingResult> SendGiftAsync(GiftItem item, string playerName);
        Task<GiftingResult> SendGiftAsync(GiftItem item, string playerName, int playerTeam);
        Task<GiftingResult> SendGiftAsync(GiftItem item, GiftTrait[] traits, string playerName);
        Task<GiftingResult> SendGiftAsync(GiftItem item, GiftTrait[] traits, string playerName, int playerTeam);
    }
}
