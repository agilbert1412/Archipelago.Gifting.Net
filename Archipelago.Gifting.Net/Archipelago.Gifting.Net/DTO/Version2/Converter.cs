using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json.Linq;

namespace Archipelago.Gifting.Net.DTO.Version2
{
    internal class Converter : IVersionedConverter<Gift, Version1.Gift>
    {
        public int Version => DataVersion.GiftDataVersion2;
        public int PreviousVersion => DataVersion.GiftDataVersion1;

        private Validator _validator;
        private IVersionedConverter<Version1.Gift, object> _previousConverter;

        public Converter()
        {
            _validator = new Validator();
            _previousConverter = new Version1.Converter();
        }

        public Dictionary<Guid, Gift> ReadFromDataStorage(DataStorageElement element)
        {
            try
            {
                var giftboxContent = element.To<Dictionary<Guid, Gift>>() ?? new Dictionary<Guid, Gift>();
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
                var previousVersionContent = _previousConverter.ReadFromDataStorage(element);
                var currentVersionContent = previousVersionContent.ToDictionary(x => x.Key, x => ConvertToCurrentVersion(x.Value));
                return currentVersionContent;
            }
        }

        public Dictionary<Guid, Gift> ReadFromDataStorage(JToken element)
        {
            try
            {
                var giftboxContent = element.ToObject<Dictionary<Guid, Gift>>() ?? new Dictionary<Guid, Gift>();
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
                var previousVersionContent = _previousConverter.ReadFromDataStorage(element);
                var currentVersionContent = previousVersionContent
                    .ToDictionary(x => x.Key, x => ConvertToCurrentVersion(x.Value));
                return currentVersionContent;
            }
        }

        public IDictionary CreateDataStorageUpdateEntry(Gift gift, int version)
        {
            if (version < Version)
            {
                return _previousConverter.CreateDataStorageUpdateEntry(ConvertToPreviousVersion(gift), version);
            }

            var newGiftEntry = new Dictionary<Guid, Gift>
            {
                { gift.ID, gift },
            };

            return newGiftEntry;
        }

        public Gift ConvertToCurrentVersion(Version1.Gift olderGift)
        {
            var currentGift = new Gift(olderGift.Item.Name, olderGift.Item.Amount, olderGift.Item.Value, olderGift.Traits,
                olderGift.SenderName, olderGift.ReceiverName, olderGift.SenderTeam, olderGift.ReceiverTeam);
            currentGift.ID = olderGift.ID;
            return currentGift;
        }

        public Version1.Gift ConvertToPreviousVersion(Gift currentGift)
        {
            var giftItem = new GiftItem(currentGift.ItemName, currentGift.Amount, currentGift.ItemValue);
            var olderGift = new Version1.Gift(giftItem, currentGift.Traits, currentGift.SenderName, currentGift.ReceiverName, currentGift.SenderTeam, currentGift.ReceiverTeam);
            olderGift.ID = currentGift.ID;
            return olderGift;
        }
    }
}
