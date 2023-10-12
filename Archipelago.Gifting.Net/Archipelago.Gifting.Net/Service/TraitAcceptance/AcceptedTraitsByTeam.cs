using System.Collections;
using System.Collections.Generic;

namespace Archipelago.Gifting.Net.Service.TraitAcceptance
{
    public class AcceptedTraitsByTeam : IDictionary<int, AcceptedTraitsByPlayer>
    {
        private IDictionary<int, AcceptedTraitsByPlayer> _traitsByTeam;

        internal AcceptedTraitsByTeam() : this(new Dictionary<int, AcceptedTraitsByPlayer>()) { }

        internal AcceptedTraitsByTeam(IDictionary<int, AcceptedTraitsByPlayer> traitsByTeam)
        {
            _traitsByTeam = traitsByTeam;
        }

        public IEnumerator<KeyValuePair<int, AcceptedTraitsByPlayer>> GetEnumerator()
        {
            return _traitsByTeam.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<int, AcceptedTraitsByPlayer> item)
        {
            _traitsByTeam.Add(item);
        }

        public void Clear()
        {
            _traitsByTeam.Clear();
        }

        public bool Contains(KeyValuePair<int, AcceptedTraitsByPlayer> item)
        {
            return _traitsByTeam.Contains(item);
        }

        public void CopyTo(KeyValuePair<int, AcceptedTraitsByPlayer>[] array, int arrayIndex)
        {
            _traitsByTeam.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<int, AcceptedTraitsByPlayer> item)
        {
            return _traitsByTeam.Remove(item);
        }

        public int Count => _traitsByTeam.Count;
        public bool IsReadOnly => _traitsByTeam.IsReadOnly;
        public bool ContainsKey(int key)
        {
            return _traitsByTeam.ContainsKey(key);
        }

        public void Add(int key, AcceptedTraitsByPlayer value)
        {
            _traitsByTeam.Add(key, value);
        }

        public bool Remove(int key)
        {
            return _traitsByTeam.Remove(key);
        }

        public bool TryGetValue(int key, out AcceptedTraitsByPlayer value)
        {
            return _traitsByTeam.TryGetValue(key, out value);
        }

        public AcceptedTraitsByPlayer this[int key]
        {
            get => _traitsByTeam[key];
            set => _traitsByTeam[key] = value;
        }

        public ICollection<int> Keys => _traitsByTeam.Keys;
        public ICollection<AcceptedTraitsByPlayer> Values => _traitsByTeam.Values;
    }
}
