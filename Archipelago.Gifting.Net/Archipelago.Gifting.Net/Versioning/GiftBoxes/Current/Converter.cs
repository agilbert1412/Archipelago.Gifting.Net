using System;
using System.Collections;
using System.Collections.Generic;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json.Linq;

namespace Archipelago.Gifting.Net.Versioning.GiftBoxes.Current
{
    internal class Converter : IVersionedGiftBoxConverter<GiftBox, Version2.GiftBox>
    {
        public int Version => DataVersion.GIFT_DATA_VERSION_3;
        public int PreviousVersion => DataVersion.GIFT_DATA_VERSION_2;
        private IVersionedGiftBoxConverter<Version2.GiftBox, object> _previousConverter;

        public Converter()
        {
            _previousConverter = new Version2.Converter();
        }

        public Dictionary<int, GiftBox> ReadFromDataStorage(DataStorageElement element)
        {
            try
            {
                var motherBoxContent = element.To<Dictionary<int, GiftBox>>() ?? new Dictionary<int, GiftBox>();
                var previousVersionContent = _previousConverter.ReadFromDataStorage(element);

                foreach (var giftBoxId in previousVersionContent.Keys)
                {
                    try
                    {
                        if (!motherBoxContent.ContainsKey(giftBoxId) || HasInvalidFields(motherBoxContent[giftBoxId]))
                        {
                            motherBoxContent[giftBoxId] = ConvertToCurrentVersion(previousVersionContent[giftBoxId]);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored, it just means at least one gift couldn't be converted. We can still attempt the rest
                    }
                }

                return motherBoxContent;
            }
            catch (Exception)
            {
                return new Dictionary<int, GiftBox>();
            }
        }

        public Dictionary<int, GiftBox> ReadFromDataStorage(JToken element)
        {
            try
            {
                var motherBoxContent = element.ToObject<Dictionary<int, GiftBox>>() ?? new Dictionary<int, GiftBox>();
                var previousVersionContent = _previousConverter.ReadFromDataStorage(element);

                foreach (var giftBoxId in previousVersionContent.Keys)
                {
                    try
                    {
                        if (!motherBoxContent.ContainsKey(giftBoxId) || HasInvalidFields(motherBoxContent[giftBoxId]))
                        {
                            motherBoxContent[giftBoxId] = ConvertToCurrentVersion(previousVersionContent[giftBoxId]);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored, it just means at least one gift couldn't be converted. We can still attempt the rest
                    }
                }

                return motherBoxContent;
            }
            catch (Exception)
            {
                return new Dictionary<int, GiftBox>();
            }
        }

        private bool HasInvalidFields(GiftBox giftBox)
        {
            return giftBox == null ||
                   giftBox.MinimumGiftDataVersion < DataVersion.FirstVersion ||
                   giftBox.MaximumGiftDataVersion < DataVersion.FirstVersion;
        }

        public IDictionary CreateDataStorageUpdateEntry(int ownerSlot, GiftBox giftBox, int version)
        {
            if (version < Version)
            {
                return _previousConverter.CreateDataStorageUpdateEntry(ownerSlot, ConvertToPreviousVersion(giftBox), version);
            }

            var newGiftBoxEntry = new Dictionary<int, GiftBox>
            {
                { ownerSlot, giftBox },
            };

            return newGiftBoxEntry;
        }

        public GiftBox ConvertToCurrentVersion(Version2.GiftBox olderGift)
        {
            var currentGiftBox = new GiftBox
            {
                AcceptsAnyGift = olderGift.AcceptsAnyGift,
                DesiredTraits = olderGift.DesiredTraits,
                IsOpen = olderGift.IsOpen,
                MaximumGiftDataVersion = olderGift.MaximumGiftDataVersion,
                MinimumGiftDataVersion = olderGift.MinimumGiftDataVersion,
            };
            return currentGiftBox;
        }

        public Version2.GiftBox ConvertToPreviousVersion(GiftBox currentGift)
        {
            var olderGiftBox = new Version2.GiftBox
            {
                AcceptsAnyGift = currentGift.AcceptsAnyGift,
                DesiredTraits = currentGift.DesiredTraits,
                IsOpen = currentGift.IsOpen,
                MaximumGiftDataVersion = currentGift.MaximumGiftDataVersion,
                MinimumGiftDataVersion = currentGift.MinimumGiftDataVersion,
            };
            return olderGiftBox;
        }
    }
}
