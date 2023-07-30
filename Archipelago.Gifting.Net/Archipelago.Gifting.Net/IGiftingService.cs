using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net.Helpers;

namespace Archipelago.Gifting.Net
{
    public interface IGiftingService
    {
        void OpenGiftBox();
        void CloseGiftBox();
        bool CanGiftToPlayer(string playerName);
        bool SendGift(GiftItem item, string playerName);
        bool SendGift(GiftItem item, string playerName, int playerTeam);
        bool SendGift(GiftItem item, GiftTrait[] traits, string playerName);
        bool SendGift(GiftItem item, GiftTrait[] traits, string playerName, int playerTeam);
        bool SendGift(GiftItem item, string playerName, out Guid giftId);
        bool SendGift(GiftItem item, string playerName, int playerTeam, out Guid giftId);
        bool SendGift(GiftItem item, GiftTrait[] traits, string playerName, out Guid giftId);
        bool SendGift(GiftItem item, GiftTrait[] traits, string playerName, int playerTeam, out Guid giftId);
        bool RefundGift(Gift gift);
        Dictionary<Guid, Gift> GetAllGiftsAndEmptyGiftbox();
        Dictionary<Guid, Gift> CheckGiftBox();
        void EmptyGiftBox();
        void RemoveGiftFromGiftBox(Guid giftId);
    }
}
