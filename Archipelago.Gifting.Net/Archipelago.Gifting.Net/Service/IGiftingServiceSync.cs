using System;
using System.Collections.Generic;
using Archipelago.Gifting.Net.Giftboxes;
using Archipelago.Gifting.Net.Gifts;
using Archipelago.Gifting.Net.Gifts.Versions.Current;
using Archipelago.Gifting.Net.Service.TraitAcceptance;
using Archipelago.Gifting.Net.Traits;

namespace Archipelago.Gifting.Net.Service
{
    public interface IGiftingServiceSync
    {
        string GetMyGiftBoxKey();
        void OpenGiftBox();
        void OpenGiftBox(bool acceptAnyGift, string[] desiredTraits);
        void CloseGiftBox();

        /// <summary>
        /// Gets the current metadata state of your own giftbox, as registered in the motherbox
        /// </summary>
        /// <returns>Your own giftbox information</returns>
        GiftBox GetCurrentGiftboxState();
        bool CanGiftToPlayer(string playerName);
        bool CanGiftToPlayer(string playerName, int playerTeam);
        bool CanGiftToPlayer(string playerName, IEnumerable<string> giftTraits);
        bool CanGiftToPlayer(string playerName, int playerTeam, IEnumerable<string> giftTraits);
        bool CanGiftToPlayer(int playerSlot);
        bool CanGiftToPlayer(int playerSlot, int playerTeam);
        bool CanGiftToPlayer(int playerSlot, IEnumerable<string> giftTraits);
        bool CanGiftToPlayer(int playerSlot, int playerTeam, IEnumerable<string> giftTraits);

        /// <summary>
        /// Provided a list of traits that you can send, get the full information, for every player in the multiworld, about which of these traits they can get
        /// </summary>
        /// <param name="giftTraits">The traits you can send</param>
        /// <returns>The accepted traits, by player, by team</returns>
        AcceptedTraitsByTeam GetAcceptedTraitsByTeam(IEnumerable<string> giftTraits);

        /// <summary>
        /// Provided a list of traits that you can send, get the full information, for every player in a specific team, about which of these traits they can get
        /// </summary>
        /// <param name="team">The team to query players from</param>
        /// <param name="giftTraits">The traits you can send</param>
        /// <returns>The accepted traits, by player</returns>
        AcceptedTraitsByPlayer GetAcceptedTraitsByPlayer(int team, IEnumerable<string> giftTraits);
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
