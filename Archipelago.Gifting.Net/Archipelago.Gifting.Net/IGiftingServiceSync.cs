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
        bool SendGift(GiftItem item, string playerName, out Guid giftId);
        bool SendGift(GiftItem item, string playerName, int playerTeam, out Guid giftId);
        bool SendGift(GiftItem item, GiftTrait[] traits, string playerName, out Guid giftId);
        bool SendGift(GiftItem item, GiftTrait[] traits, string playerName, int playerTeam, out Guid giftId);
        bool RefundGift(Gift gift);
        Dictionary<Guid, Gift> GetAllGiftsAndEmptyGiftbox();
        Dictionary<Guid, Gift> CheckGiftBox();
        void RemoveGiftsFromGiftBox(IEnumerable<Guid> giftIds);
        void RemoveGiftFromGiftBox(Guid giftId);
        void SubscribeToNewGifts(Action<Dictionary<Guid, Gift>> newGiftsCallback);
    }
}
