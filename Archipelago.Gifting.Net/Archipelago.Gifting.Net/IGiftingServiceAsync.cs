using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Archipelago.Gifting.Net.DTO.Version2;

namespace Archipelago.Gifting.Net
{
    public interface IGiftingServiceAsync
    {
        Task<Dictionary<string, Gift>> CheckGiftBoxAsync();
        Task<bool> CanGiftToPlayerAsync(int playerSlot, int playerTeam, IEnumerable<string> giftTraits);
        Task<bool> SendGiftAsync(GiftItem item, GiftTrait[] traits, string playerName, int playerTeam);
    }
}
