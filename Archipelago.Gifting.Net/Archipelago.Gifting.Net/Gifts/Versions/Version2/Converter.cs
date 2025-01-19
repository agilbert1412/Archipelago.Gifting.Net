using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Archipelago.Gifting.Net.Utilities;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json.Linq;

namespace Archipelago.Gifting.Net.Gifts.Versions.Version2
{
    internal class Converter : IVersionedConverter<Gift, Version1.Gift>
    {
        public int Version => DataVersion.GIFT_DATA_VERSION_2;
        public int PreviousVersion => DataVersion.GIFT_DATA_VERSION_1;

        private PlayerProvider _playerProvider;
        private Validator _validator;
        private IVersionedConverter<Version1.Gift, object> _previousConverter;

        public Converter(PlayerProvider playerProvider)
        {
            _playerProvider = playerProvider;
            _validator = new Validator();
            _previousConverter = new Version1.Converter();
        }

        public Dictionary<string, Gift> ReadFromDataStorage(DataStorageElement element)
        {
            try
            {
                var giftboxContent = element.To<Dictionary<string, Gift>>() ?? new Dictionary<string, Gift>();
                if (_validator.Validate(giftboxContent, out var errorIds))
                {
                    return giftboxContent;
                }

                var previousVersionContent = _previousConverter.ReadFromDataStorage(element);
                foreach (var previousIdGift in previousVersionContent)
                {
                    var id = previousIdGift.Key;
                    var previousGift = previousIdGift.Value;
                    if (!giftboxContent.ContainsKey(id) || errorIds.Contains(id))
                    {
                        giftboxContent[id] = ConvertToCurrentVersion(previousGift);
                    }
                }

                return giftboxContent;

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
                var giftboxContent = element.ToObject<Dictionary<string, Gift>>() ?? new Dictionary<string, Gift>();
                if (_validator.Validate(giftboxContent, out var errorIds))
                {
                    return giftboxContent;
                }

                var previousVersionContent = _previousConverter.ReadFromDataStorage(element);
                foreach (var previousIdGift in previousVersionContent)
                {
                    var id = previousIdGift.Key;
                    var previousGift = previousIdGift.Value;
                    if (!giftboxContent.ContainsKey(id) || errorIds.Contains(id))
                    {
                        giftboxContent[id] = ConvertToCurrentVersion(previousGift);
                    }
                }

                return giftboxContent;
            }
            catch (Exception)
            {
                return new Dictionary<string, Gift>();
            }
        }

        public IDictionary CreateDataStorageUpdateEntry(Gift gift, int version)
        {
            if (version < Version)
            {
                return _previousConverter.CreateDataStorageUpdateEntry(ConvertToPreviousVersion(gift), version);
            }

            var newGiftEntry = new Dictionary<string, Gift>
            {
                { gift.ID, gift },
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
