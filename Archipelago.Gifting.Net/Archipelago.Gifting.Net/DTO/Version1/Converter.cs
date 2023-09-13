using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json.Linq;

namespace Archipelago.Gifting.Net.DTO.Version1
{
    internal class Converter : IVersionedConverter<Gift, object>
    {
        public int Version => DataVersion.GIFT_DATA_VERSION_1;
        public int PreviousVersion => Version - 1;

        public Converter()
        {
        }

        public Dictionary<string, Gift> ReadFromDataStorage(DataStorageElement element)
        {
            try
            {
                var giftboxContent = element.To<Dictionary<string, Gift>>() ?? new Dictionary<string, Gift>();
                return giftboxContent.ToDictionary(x => x.Key.ToString(), x => x.Value);
            }
            catch (Exception ex)
            {
                return new Dictionary<string, Gift>();
            }
        }

        public Dictionary<string, Gift> ReadFromDataStorage(JToken element)
        {
            try
            {
                var giftboxContent = element.ToObject<Dictionary<string, Gift>>() ?? new Dictionary<string, Gift>();
                return giftboxContent.ToDictionary(x => x.Key.ToString(), x => x.Value);
            }
            catch (Exception ex)
            {
                return new Dictionary<string, Gift>();
            }
        }

        public IDictionary CreateDataStorageUpdateEntry(Gift gift, int version)
        {
            if (version < Version)
            {
                throw new VersionNotFoundException($"Tried to create a gift for an unknown version: {version}");
            }

            var newGiftEntry = new Dictionary<Guid, Gift>
            {
                { Guid.Parse(gift.ID), gift },
            };

            return newGiftEntry;
        }

        public Gift ConvertToCurrentVersion(object olderGift)
        {
            throw new NotImplementedException();
        }

        public object ConvertToPreviousVersion(Gift currentGift)
        {
            throw new NotImplementedException();
        }
    }
}
