using System;
using System.Collections.Generic;
using Archipelago.Gifting.Net.Service.Result;
using Archipelago.Gifting.Net.Service.TraitAcceptance;
using Archipelago.Gifting.Net.Versioning.GiftBoxes.Current;
using Archipelago.Gifting.Net.Versioning.Gifts;
using Archipelago.Gifting.Net.Versioning.Gifts.Current;

namespace Archipelago.Gifting.Net.Service
{
    public interface IGiftingServiceSync
    {
        string GetMyGiftBoxKey();
        void OpenGiftBox();
        void OpenGiftBox(bool acceptAnyGift, string[] desiredTraits);
        void CloseGiftBox();

        /// <summary>
        /// Gets the current metadata state of your own giftBox, as registered in the motherbox
        /// </summary>
        /// <returns>Your own giftBox information</returns>
        GiftBox GetCurrentGiftBoxState();

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

        GiftingResult SendGift(GiftItem item, string playerName);
        GiftingResult SendGift(GiftItem item, string playerName, int playerTeam);
        GiftingResult SendGift(GiftItem item, GiftTrait[] traits, string playerName);
        GiftingResult SendGift(GiftItem item, GiftTrait[] traits, string playerName, int playerTeam);
        GiftingResult RefundGift(Gift gift);
        Dictionary<string, Gift> GetAllGiftsAndEmptyGiftBox();
        Dictionary<string, Gift> CheckGiftBox();
        void RemoveGiftsFromGiftBox(IEnumerable<string> giftIds);
        void RemoveGiftFromGiftBox(string giftId);

        [Obsolete("SubscribeToNewGifts is deprecated. Instead, use the event OnNewGift and subscribe by adding the handlers you need")]
        void SubscribeToNewGifts(Action<Dictionary<string, Gift>> newGiftsCallback);

        #region Obsolete Stuff

        [Obsolete("The overloads with out parameters are now deprecated, please use the overloads that return a GiftingResult instead")]
        bool SendGift(GiftItem item, string playerName, out string giftId);

        [Obsolete("The overloads with out parameters are now deprecated, please use the overloads that return a GiftingResult instead")]
        bool SendGift(GiftItem item, string playerName, int playerTeam, out string giftId);

        [Obsolete("The overloads with out parameters are now deprecated, please use the overloads that return a GiftingResult instead")]
        bool SendGift(GiftItem item, GiftTrait[] traits, string playerName, out string giftId);

        [Obsolete("The overloads with out parameters are now deprecated, please use the overloads that return a GiftingResult instead")]
        bool SendGift(GiftItem item, GiftTrait[] traits, string playerName, int playerTeam, out string giftId);

        #endregion Obsolete Stuff
    }
}
