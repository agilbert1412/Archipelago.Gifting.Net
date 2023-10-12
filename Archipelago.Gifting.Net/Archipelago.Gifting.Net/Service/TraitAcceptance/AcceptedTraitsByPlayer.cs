using System.Collections;
using System.Collections.Generic;

namespace Archipelago.Gifting.Net.Service.TraitAcceptance
{
    public class AcceptedTraitsByPlayer : IDictionary<int, AcceptedTraits>
    {
        private IDictionary<int, AcceptedTraits> _traitsByPlayer;

        internal AcceptedTraitsByPlayer() : this(new Dictionary<int, AcceptedTraits>()) { }

        internal AcceptedTraitsByPlayer(IDictionary<int, AcceptedTraits> traitsByPlayer)
        {
            _traitsByPlayer = traitsByPlayer;
        }

        public IEnumerator<KeyValuePair<int, AcceptedTraits>> GetEnumerator()
        {
            return _traitsByPlayer.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<int, AcceptedTraits> item)
        {
            _traitsByPlayer.Add(item);
        }

        public void Clear()
        {
            _traitsByPlayer.Clear();
        }

        public bool Contains(KeyValuePair<int, AcceptedTraits> item)
        {
            return _traitsByPlayer.Contains(item);
        }

        public void CopyTo(KeyValuePair<int, AcceptedTraits>[] array, int arrayIndex)
        {
            _traitsByPlayer.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<int, AcceptedTraits> item)
        {
            return _traitsByPlayer.Remove(item);
        }

        public int Count => _traitsByPlayer.Count;
        public bool IsReadOnly => _traitsByPlayer.IsReadOnly;
        public bool ContainsKey(int key)
        {
            return _traitsByPlayer.ContainsKey(key);
        }

        public void Add(int key, AcceptedTraits value)
        {
            _traitsByPlayer.Add(key, value);
        }

        public bool Remove(int key)
        {
            return _traitsByPlayer.Remove(key);
        }

        public bool TryGetValue(int key, out AcceptedTraits value)
        {
            return _traitsByPlayer.TryGetValue(key, out value);
        }

        public AcceptedTraits this[int key]
        {
            get => _traitsByPlayer[key];
            set => _traitsByPlayer[key] = value;
        }

        public ICollection<int> Keys => _traitsByPlayer.Keys;
        public ICollection<AcceptedTraits> Values => _traitsByPlayer.Values;
    }
}
