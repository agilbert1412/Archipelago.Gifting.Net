using System;
using System.Collections.Generic;
using Archipelago.Gifting.Net.DTO.Version2;

namespace Archipelago.Gifting.Net
{
    public interface IGiftingServiceSync
    {
        string GetMyGiftBoxKey();
        void OpenGiftBox();
        void OpenGiftBox(bool acceptAnyGift, string[] desiredTraits);
        void CloseGiftBox();
        bool CanGiftToPlayer(string playerName);
        bool CanGiftToPlayer(string playerName, int playerTeam);
        bool CanGiftToPlayer(string playerName, IEnumerable<string> giftTraits);
        bool CanGiftToPlayer(string playerName, int playerTeam, IEnumerable<string> giftTraits);
        bool CanGiftToPlayer(int playerSlot);
        bool CanGiftToPlayer(int playerSlot, int playerTeam);
        bool CanGiftToPlayer(int playerSlot, IEnumerable<string> giftTraits);
        bool CanGiftToPlayer(int playerSlot, int playerTeam, IEnumerable<string> giftTraits);
        bool SendGift(GiftItem item, string playerName);
        bool SendGift(GiftItem item, string playerName, int playerTeam);
        bool SendGift(GiftItem item, GiftTrait[] traits, string playerName);
        bool SendGift(GiftItem item, GiftTrait[] traits, string playerName, int playerTeam);
        bool SendGift(GiftItem item, string playerName, out string giftId);
        bool SendGift(GiftItem item, string playerName, int playerTeam, out string giftId);
        bool SendGift(GiftItem item, GiftTrait[] traits, string playerName, out string giftId);
        bool SendGift(GiftItem item, GiftTrait[] traits, string playerName, int playerTeam, out string giftId);
        bool RefundGift(Gift gift);
        Dictionary<string, Gift> GetAllGiftsAndEmptyGiftbox();
        Dictionary<string, Gift> CheckGiftBox();
        void RemoveGiftsFromGiftBox(IEnumerable<string> giftIds);
        void RemoveGiftFromGiftBox(string giftId);
        void SubscribeToNewGifts(Action<Dictionary<string, Gift>> newGiftsCallback);
    }
}
