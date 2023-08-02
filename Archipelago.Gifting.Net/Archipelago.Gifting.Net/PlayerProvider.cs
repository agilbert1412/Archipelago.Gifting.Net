using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Helpers;

namespace Archipelago.Gifting.Net
{
    public class PlayerProvider
    {
        private ArchipelagoSession _session;
        public PlayerInfo CurrentPlayer => GetPlayer(_session.ConnectionInfo.Slot);
        public string CurrentPlayerName => CurrentPlayer.Name;
        public int CurrentPlayerTeam => _session.ConnectionInfo.Team;
        public int CurrentPlayerSlot => _session.ConnectionInfo.Slot;
        public string CurrentPlayerGame => _session.ConnectionInfo.Game;

        public PlayerProvider(ArchipelagoSession session)
        {
            _session = session;
        }

        public bool TryGetPlayer(string playerName, out PlayerInfo player)
        {
            return TryGetPlayer(playerName, CurrentPlayerTeam, out player);
        }

        public bool TryGetPlayer(string playerName, int playerTeam, out PlayerInfo player)
        {
            foreach (var teamPlayer in _session.Players.Players[playerTeam])
            {
                if (teamPlayer.Name == playerName)
                {
                    player = teamPlayer;
                    return true;
                }
            }

            player = null;
            var numberMatchingAliases = 0;
            foreach (var teamPlayer in _session.Players.Players[playerTeam])
            {
                if (teamPlayer.Alias == playerName)
                {
                    numberMatchingAliases++;
                    player = teamPlayer;
                }
            }
            
            return numberMatchingAliases == 1;
        }

        public PlayerInfo GetPlayer(string playerName)
        {
            return GetPlayer(playerName, CurrentPlayerTeam);
        }

        public PlayerInfo GetPlayer(string playerName, int playerTeam)
        {
            return _session.Players.Players[playerTeam].First(player => player.Name == playerName);
        }

        public PlayerInfo GetPlayer(int playerSlot)
        {
            return GetPlayer(playerSlot, CurrentPlayerTeam);
        }

        public PlayerInfo GetPlayer(int playerSlot, int playerTeam)
        {
            return _session.Players.Players[playerTeam].First(player => player.Slot == playerSlot);
        }
    }
}
