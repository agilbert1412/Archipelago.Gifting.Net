using System.Collections;
using System.Collections.Generic;

namespace Archipelago.Gifting.Net.Service.TraitAcceptance
{
    public class AcceptedTraits : IEnumerable<string>
    {
        private IEnumerable<string> _traits;

        internal AcceptedTraits() : this(new string[0]) { }

        internal AcceptedTraits(IEnumerable<string> traits)
        {
            _traits = traits;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _traits.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}