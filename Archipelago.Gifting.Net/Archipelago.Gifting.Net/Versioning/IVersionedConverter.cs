using System.Collections;
using System.Collections.Generic;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json.Linq;

namespace Archipelago.Gifting.Net.Versioning
{
    internal interface IVersionedConverter<TKey, TCurrentObject, TPreviousObject>
    {
        int Version { get; }
        int PreviousVersion { get; }
        Dictionary<TKey, TCurrentObject> ReadFromDataStorage(DataStorageElement element);
        Dictionary<TKey, TCurrentObject> ReadFromDataStorage(JToken element);
        TCurrentObject ConvertToCurrentVersion(TPreviousObject olderItem);
        TPreviousObject ConvertToPreviousVersion(TCurrentObject currentItem);
        IDictionary CreateDataStorageUpdateEntry(TKey key, TCurrentObject item, int version);
    }
}
