using System;
using System.Collections;
using System.Collections.Generic;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json.Linq;

namespace Archipelago.Gifting.Net.Gifts.Versions
{
    internal interface IVersionedConverter<TCurrentGift, TPreviousGift>
    {
        int Version { get; }
        int PreviousVersion { get; }
        Dictionary<string, TCurrentGift> ReadFromDataStorage(DataStorageElement element);
        Dictionary<string, TCurrentGift> ReadFromDataStorage(JToken element);
        TCurrentGift ConvertToCurrentVersion(TPreviousGift olderGift);
        TPreviousGift ConvertToPreviousVersion(TCurrentGift currentGift);
        IDictionary CreateDataStorageUpdateEntry(TCurrentGift gift, int version);
    }
}
