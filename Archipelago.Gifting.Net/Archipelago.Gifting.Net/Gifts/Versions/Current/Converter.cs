using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Archipelago.Gifting.Net.Utilities;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json.Linq;

namespace Archipelago.Gifting.Net.Gifts.Versions.Current
{
    internal class Converter : IVersionedConverter<Gift, Version2.Gift>
    {
        public int Version => DataVersion.GIFT_DATA_VERSION_3;
        public int PreviousVersion => DataVersion.GIFT_DATA_VERSION_2;

        private readonly Validator _validator;
        private readonly IVersionedConverter<Version2.Gift, Version1.Gift> _previousConverter;

        public Converter(PlayerProvider playerProvider)
        {
            _validator = new Validator();
            _previousConverter = new Version2.Converter(playerProvider);
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
                    try
                    {
                        var id = previousIdGift.Key;
                        var previousGift = previousIdGift.Value;
                        if (!giftboxContent.ContainsKey(id) || errorIds.Contains(id))
                        {
                            giftboxContent[id] = ConvertToCurrentVersion(previousGift);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored, it just means at least one gift couldn't be converted. We can still attempt the rest
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

            var jsonObject = JObject.FromObject(gift);
            foreach (var property in jsonObject.Properties())
            {
                if (property.Name != "traits")
                {
                    continue;
                }

                var traits = property.Value;
                foreach (var trait in traits)
                {
                    RemoveDefaultValueProperties(trait, new[] { "quality", "duration" }, 1.0);
                }
            }

            var newGiftEntry = new Dictionary<string, JObject>
            {
                { gift.id, jsonObject },
            };

            return newGiftEntry;
        }

        private static void RemoveDefaultValueProperties(JToken trait, string[] propertiesToCheck, double valueToOmit)
        {
            foreach (var propertyName in propertiesToCheck)
            {
                var quality = trait[propertyName];
                if (quality != null && IsDefaultValue(quality, valueToOmit))
                {
                    quality.Parent.Remove();
                }
            }
        }

        private static bool IsDefaultValue(JToken quality, double defaultValue)
        {
            const double epsilon = 0.001;
            return Math.Abs(quality.Value<double>() - defaultValue) < epsilon;
        }

        public Gift ConvertToCurrentVersion(Version2.Gift olderGift)
        {
            var currentGift = new Gift(olderGift.ItemName,
                olderGift.Amount,
                olderGift.ItemValue,
                ConvertTraits(olderGift.Traits),
                olderGift.SenderSlot,
                olderGift.ReceiverSlot,
                olderGift.SenderTeam,
                olderGift.ReceiverTeam);
            currentGift.id = olderGift.ID;
            return currentGift;
        }

        public Version2.Gift ConvertToPreviousVersion(Gift currentGift)
        {
            var olderGift = new Version2.Gift(currentGift.itemName, currentGift.amount, currentGift.itemValue, ConvertTraits(currentGift.traits), currentGift.senderSlot,
                currentGift.receiverSlot, currentGift.senderTeam, currentGift.receiverTeam);
            olderGift.ID = currentGift.id;
            return olderGift;
        }

        private GiftTrait[] ConvertTraits(Version2.GiftTrait[] olderGiftTraits)
        {
            return olderGiftTraits.Select(x => new GiftTrait(x.Trait, x.Duration, x.Quality)).ToArray();
        }

        private Version2.GiftTrait[] ConvertTraits(GiftTrait[] newerGiftTraits)
        {
            return newerGiftTraits.Select(x => new Version2.GiftTrait(x.trait, x.duration, x.quality)).ToArray();
        }
    }
}
