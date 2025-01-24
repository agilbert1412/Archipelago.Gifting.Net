using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json.Linq;

namespace Archipelago.Gifting.Net.Versioning.GiftBoxes.Version2
{
    internal class Converter : IVersionedGiftBoxConverter<GiftBox, object>
    {
        public int Version => DataVersion.GIFT_DATA_VERSION_1;
        public int PreviousVersion => Version - 1;

        public Converter()
        {
        }

        public Dictionary<int, GiftBox> ReadFromDataStorage(DataStorageElement element)
        {
            try
            {
                var motherBoxContent = element.To<Dictionary<int, GiftBox>>() ?? new Dictionary<int, GiftBox>();
                return motherBoxContent;
            }
            catch (Exception ex)
            {
                return new Dictionary<int, GiftBox>();
            }
        }

        public Dictionary<int, GiftBox> ReadFromDataStorage(JToken element)
        {
            try
            {
                var motherBoxContent = element.ToObject<Dictionary<int, GiftBox>>() ?? new Dictionary<int, GiftBox>();
                return motherBoxContent;
            }
            catch (Exception ex)
            {
                return new Dictionary<int, GiftBox>();
            }
        }

        public IDictionary CreateDataStorageUpdateEntry(int ownerSlot, GiftBox giftBox, int version)
        {
            if (version < Version)
            {
                throw new VersionNotFoundException($"Tried to create a giftBox for an unknown version: {version}");
            }

            var newGiftBoxEntry = new Dictionary<int, GiftBox>
            {
                { ownerSlot, giftBox },
            };

            return newGiftBoxEntry;
        }

        public GiftBox ConvertToCurrentVersion(object olderGiftBox)
        {
            throw new NotImplementedException();
        }

        public object ConvertToPreviousVersion(GiftBox currentGiftBox)
        {
            throw new NotImplementedException();
        }
    }
}
