using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Archipelago.Gifting.Net.Service.TraitAcceptance
{
    public class AcceptedTraits
    {
        public int Team { get; }
        public int Player { get; }
        public string[] Traits { get; }

        internal AcceptedTraits(int team, int player) : this(team, player, new string[0]) { }

        internal AcceptedTraits(int team, int player, string[] traits)
        {
            Team = team;
            Player = player;
            Traits = traits;
        }

        public bool Any()
        {
            return Traits.Any();
        }
    }
}