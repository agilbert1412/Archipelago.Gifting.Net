using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Archipelago.Gifting.Net.Utilities;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json.Linq;

namespace Archipelago.Gifting.Net.Versioning.Gifts.Version2
{
    internal class GiftConverter : IVersionedGiftConverter<Gift, Version1.Gift>
    {
        public int Version => DataVersion.GIFT_DATA_VERSION_2;
        public int PreviousVersion => DataVersion.GIFT_DATA_VERSION_1;

        private PlayerProvider _playerProvider;
        private Validator _validator;
        private IVersionedGiftConverter<Version1.Gift, object> _previousConverter;

        public GiftConverter(PlayerProvider playerProvider)
        {
            _playerProvider = playerProvider;
            _validator = new Validator();
            _previousConverter = new Version1.GiftConverter();
        }

        public Dictionary<string, Gift> ReadFromDataStorage(DataStorageElement element)
        {
            try
            {
                var giftBoxContent = element.To<Dictionary<string, Gift>>() ?? new Dictionary<string, Gift>();
                if (_validator.Validate(giftBoxContent, out var errorIds))
                {
                    return giftBoxContent;
                }

                var previousVersionContent = _previousConverter.ReadFromDataStorage(element);
                foreach (var previousIdGift in previousVersionContent)
                {
                    try
                    {
                        var id = previousIdGift.Key;
                        var previousGift = previousIdGift.Value;
                        if (!giftBoxContent.ContainsKey(id) || errorIds.Contains(id))
                        {
                            giftBoxContent[id] = ConvertToCurrentVersion(previousGift);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored, it just means at least one gift couldn't be converted. We can still attempt the rest
                    }
                }

                return giftBoxContent;
            }
            catch (Exception)
            {
                return new Dictionary<string, Gift>();
            }
        }

        public Dictionary<string, Gift> ReadFromDataStorage(JToken element)
        {
            try
            {
                var giftBoxContent = element.ToObject<Dictionary<string, Gift>>() ?? new Dictionary<string, Gift>();
                if (_validator.Validate(giftBoxContent, out var errorIds))
                {
                    return giftBoxContent;
                }

                var previousVersionContent = _previousConverter.ReadFromDataStorage(element);
                foreach (var previousIdGift in previousVersionContent)
                {
                    var id = previousIdGift.Key;
                    var previousGift = previousIdGift.Value;
                    if (!giftBoxContent.ContainsKey(id) || errorIds.Contains(id))
                    {
                        giftBoxContent[id] = ConvertToCurrentVersion(previousGift);
                    }
                }

                return giftBoxContent;
            }
            catch (Exception)
            {
                return new Dictionary<string, Gift>();
            }
        }

        public IDictionary CreateDataStorageUpdateEntry(string id, Gift gift, int version)
        {
            if (version < Version)
            {
                return _previousConverter.CreateDataStorageUpdateEntry(id, ConvertToPreviousVersion(gift), version);
            }

            var newGiftEntry = new Dictionary<string, Gift>
            {
                { id, gift },
            };

            return newGiftEntry;
        }

        public Gift ConvertToCurrentVersion(Version1.Gift olderGift)
        {
            var sender = _playerProvider.GetPlayer(olderGift.SenderName, olderGift.SenderTeam);
            var receiver = _playerProvider.GetPlayer(olderGift.ReceiverName, olderGift.ReceiverTeam);
            var currentGift = new Gift(olderGift.Item.Name,
                olderGift.Item.Amount,
                olderGift.Item.Value,
                ConvertTraits(olderGift.Traits),
                sender.Slot,
                receiver.Slot,
                olderGift.SenderTeam,
                olderGift.ReceiverTeam);
            currentGift.ID = olderGift.ID;
            return currentGift;
        }

        public Version1.Gift ConvertToPreviousVersion(Gift currentGift)
        {
            var giftItem = new GiftItem(currentGift.ItemName, currentGift.Amount, currentGift.ItemValue);
            var sender = _playerProvider.GetPlayer(currentGift.SenderSlot, currentGift.SenderTeam);
            var receiver = _playerProvider.GetPlayer(currentGift.ReceiverSlot, currentGift.ReceiverTeam);
            var olderGift = new Version1.Gift(giftItem, ConvertTraits(currentGift.Traits), sender.Name, receiver.Name, currentGift.SenderTeam, currentGift.ReceiverTeam);
            olderGift.ID = currentGift.ID;
            return olderGift;
        }

        private GiftTrait[] ConvertTraits(Version1.GiftTrait[] olderGiftTraits)
        {
            return olderGiftTraits.Select(x => new GiftTrait(x.Trait, x.Duration, x.Quality)).ToArray();
        }

        private Version1.GiftTrait[] ConvertTraits(GiftTrait[] newerGiftTraits)
        {
            return newerGiftTraits.Select(x => new Version1.GiftTrait(x.Trait, x.Duration, x.Quality)).ToArray();
        }
    }
}
