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

        public PlayerInfo GetPlayer(string playerName)
        {
            return GetPlayer(playerName, CurrentPlayerTeam);
        }

        public PlayerInfo GetPlayer(string playerName, int playerTeam)
        {
            return _session.Players.Players[playerTeam].First(player => player.Name == playerName || player.Alias == playerName);
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
