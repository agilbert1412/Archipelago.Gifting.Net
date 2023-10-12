using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Helpers;

namespace Archipelago.Gifting.Net.Utilities
{
    internal class PlayerProvider
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
            const string aliasRegex = @"(.+) \((.+)\)";
            foreach (var teamPlayer in _session.Players.Players[playerTeam])
            {
                var match = Regex.Match(teamPlayer.Alias, aliasRegex);
                if (!match.Success || match.Groups.Count < 3)
                {
                    continue;
                }
                var alias = match.Groups[1].Value;
                if (alias == playerName)
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

        public IEnumerable<int> GetAllTeams()
        {
            return _session.Players.Players.Keys;
        }

        public IEnumerable<int> GetAllPlayerSlotsInTeam(int playerTeam)
        {
            return _session.Players.Players[playerTeam].Select(x => x.Slot);
        }
    }
}
