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
        public void OpenGiftBox();
        public void CloseGiftBox();
        public bool CanGiftToPlayer(string playerName);
        public bool SendGift(GiftItem item, string playerName);
        public bool SendGift(GiftItem item, string playerName, int playerTeam);
        public bool SendGift(GiftItem item, GiftTrait[] traits, string playerName);
        public bool SendGift(GiftItem item, GiftTrait[] traits, string playerName, int playerTeam);
        public bool SendGift(GiftItem item, string playerName, out Guid giftId);
        public bool SendGift(GiftItem item, string playerName, int playerTeam, out Guid giftId);
        public bool SendGift(GiftItem item, GiftTrait[] traits, string playerName, out Guid giftId);
        public bool SendGift(GiftItem item, GiftTrait[] traits, string playerName, int playerTeam, out Guid giftId);
        public bool RefundGift(Gift gift);
        public Gift[]? GetAllGiftsAndEmptyGiftbox();
        public Gift[]? CheckGiftBox();
        public void EmptyGiftBox();
    }
}
