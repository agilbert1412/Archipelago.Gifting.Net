using Archipelago.Gifting.Net.Utilities;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Archipelago.Gifting.Net.Service.TraitAcceptance;
using Archipelago.Gifting.Net.Service.Result;
using Archipelago.Gifting.Net.Versioning;
using Archipelago.Gifting.Net.Versioning.GiftBoxes;
using Archipelago.Gifting.Net.Versioning.GiftBoxes.Current;
using Archipelago.Gifting.Net.Versioning.Gifts;
using Archipelago.Gifting.Net.Versioning.Gifts.Current;

namespace Archipelago.Gifting.Net.Service
{
    public class GiftingService : IGiftingService
    {
        private ArchipelagoSession _session;
        private PlayerProvider _playerProvider;
        private GiftBoxKeyProvider _keyProvider;
        private GiftBoxConverter _currentGiftBoxConverter;
        private GiftConverter _currentGiftConverter;

        private JToken EmptyMotherboxDictionary => JToken.FromObject(new Dictionary<int, GiftBox>());
        private JToken EmptyGiftDictionary => JToken.FromObject(new Dictionary<string, Gift>());

        internal PlayerProvider PlayerProvider => _playerProvider;

        public GiftingService(ArchipelagoSession session)
        {
            _session = session;
            _playerProvider = new PlayerProvider(_session);
            _keyProvider = new GiftBoxKeyProvider(_session, _playerProvider);
            _currentGiftBoxConverter = new GiftBoxConverter();
            _currentGiftConverter = new GiftConverter(_playerProvider);
            var motherboxKey = _keyProvider.GetMotherBoxDataStorageKey();
            CreateMotherboxIfNeeded(motherboxKey);
        }

        public string GetMyGiftBoxKey()
        {
            return _keyProvider.GetGiftBoxDataStorageKey();
        }

        /// <summary>
        /// Open a giftBox that can accept any gift with no trait preference
        /// </summary>
        public void OpenGiftBox()
        {
            UpdateGiftBox(new GiftBox(true));
        }

        /// <summary>
        /// Open a giftBox with custom acceptance preference
        /// </summary>
        /// <param name="acceptAnyGift">Whether this giftBox can accept any gift, or only gifts with the specified traits</param>
        /// <param name="desiredTraits">If can accept any gift, these are traits that make for a "good" gift. If not, these are the only accepted traits</param>
        public void OpenGiftBox(bool acceptAnyGift, string[] desiredTraits)
        {
            UpdateGiftBox(new GiftBox(acceptAnyGift, desiredTraits));
        }

        public void CloseGiftBox()
        {
            UpdateGiftBox(new GiftBox(false));
            EmptyGiftBox();
        }

        public GiftBox GetCurrentGiftBoxState()
        {
            var team = _playerProvider.CurrentPlayerTeam;
            var player = _playerProvider.CurrentPlayerSlot;
            var motherBox = GetMotherbox(team);
            if (!motherBox.ContainsKey(player))
            {
                return null;
            }

            var giftBox = motherBox[player];
            return giftBox;
        }

        internal void UpdateGiftBox(GiftBox entry)
        {
            var motherboxKey = _keyProvider.GetMotherBoxDataStorageKey();
            var myGiftBoxEntry = new Dictionary<int, GiftBox>
            {
                {_playerProvider.CurrentPlayerSlot, entry},
            };
            _session.DataStorage[Scope.Global, motherboxKey] += Operation.Update(myGiftBoxEntry);
        }

        public AcceptedTraitsByTeam GetAcceptedTraitsByTeam(IEnumerable<string> giftTraits)
        {
            var acceptedTraitsByTeam = new AcceptedTraitsByTeam();
            foreach (var team in _playerProvider.GetAllTeams())
            {
                var acceptedTraitsByPlayer = GetAcceptedTraitsByPlayer(team, giftTraits);
                if (acceptedTraitsByPlayer.Any())
                {
                    acceptedTraitsByTeam.Add(team, acceptedTraitsByPlayer);
                }
            }

            return acceptedTraitsByTeam;
        }

        public AcceptedTraitsByPlayer GetAcceptedTraitsByPlayer(int team, IEnumerable<string> giftTraits)
        {
            var acceptedTraitsByPlayer = new AcceptedTraitsByPlayer();
            var teamMotherbox = GetMotherbox(team);
            if (teamMotherbox == null || !teamMotherbox.Any())
            {
                return acceptedTraitsByPlayer;
            }

            foreach (var playerGiftBox in teamMotherbox)
            {
                var player = playerGiftBox.Key;
                var giftBox = playerGiftBox.Value;
                var acceptedTraits = GetAcceptedTraits(team, player, giftBox, giftTraits);
                if (acceptedTraits.Any())
                {
                    acceptedTraitsByPlayer.Add(player, acceptedTraits);
                }
            }

            return acceptedTraitsByPlayer;
        }

        public AcceptedTraits GetAcceptedTraits(int player, IEnumerable<string> giftTraits)
        {
            return GetAcceptedTraits(_session.ConnectionInfo.Team, player, giftTraits);
        }

        public AcceptedTraits GetAcceptedTraits(int team, int player, IEnumerable<string> giftTraits)
        {
            var teamMotherbox = GetMotherbox(team);
            if (teamMotherbox == null || !teamMotherbox.Any())
            {
                return new AcceptedTraits(team, player);
            }

            var giftBox = teamMotherbox[player];
            return GetAcceptedTraits(team, player, giftBox, giftTraits);
        }

        private static AcceptedTraits GetAcceptedTraits(int team, int player, GiftBox giftBox, IEnumerable<string> giftTraits)
        {
            if (giftBox == null || !giftBox.IsOpen)
            {
                return new AcceptedTraits(team, player);
            }

            var traits = giftTraits.Where(x => giftBox.AcceptsAnyGift || giftBox.DesiredTraits.Contains(x));
            var acceptedTraits = new AcceptedTraits(team, player, traits.ToArray());
            return acceptedTraits;
        }

        public AcceptedTraits GetDesiredTraits(int player)
        {
            return GetDesiredTraits(_session.ConnectionInfo.Team, player);
        }

        public AcceptedTraits GetDesiredTraits(int team, int player)
        {
            var teamMotherbox = GetMotherbox(team);
            if (teamMotherbox == null || !teamMotherbox.Any())
            {
                return new AcceptedTraits(team, player);
            }

            var giftBox = teamMotherbox[player];
            return GetDesiredTraits(team, player, giftBox);
        }

        private static AcceptedTraits GetDesiredTraits(int team, int player, GiftBox giftBox)
        {
            if (giftBox == null || !giftBox.IsOpen)
            {
                return new AcceptedTraits(team, player);
            }

            var acceptedTraits = new AcceptedTraits(team, player, giftBox.DesiredTraits.ToArray());
            return acceptedTraits;
        }

        public GiftingResult SendGift(GiftItem item, string playerName)
        {
            return SendGift(item, new GiftTrait[0], playerName);
        }

        public async Task<GiftingResult> SendGiftAsync(GiftItem item, string playerName)
        {
            return await SendGiftAsync(item, new GiftTrait[0], playerName);
        }

        public GiftingResult SendGift(GiftItem item, GiftTrait[] traits, string playerName)
        {
            return SendGift(item, traits, playerName, _session.ConnectionInfo.Team);
        }

        public async Task<GiftingResult> SendGiftAsync(GiftItem item, GiftTrait[] traits, string playerName)
        {
            return await SendGiftAsync(item, traits, playerName, _session.ConnectionInfo.Team);
        }

        public GiftingResult SendGift(GiftItem item, string playerName, int playerTeam)
        {
            return SendGift(item, new GiftTrait[0], playerName, playerTeam);
        }

        public async Task<GiftingResult> SendGiftAsync(GiftItem item, string playerName, int playerTeam)
        {
            return await SendGiftAsync(item, new GiftTrait[0], playerName, playerTeam);
        }

        public GiftingResult SendGift(GiftItem item, GiftTrait[] traits, string playerName, int playerTeam)
        {
            var canGift = CanGiftToPlayer(playerName, playerTeam, traits.Select(x => x.Trait));
            return SendGift(item, traits, playerName, playerTeam, canGift);
        }

        public async Task<GiftingResult> SendGiftAsync(GiftItem item, GiftTrait[] traits, string playerName, int playerTeam)
        {
            var canGift = await CanGiftToPlayerAsync(playerName, playerTeam, traits.Select(x => x.Trait));
            return SendGift(item, traits, playerName, playerTeam, canGift);
        }

        private GiftingResult SendGift(GiftItem item, GiftTrait[] traits, string playerName, int playerTeam, CanGiftResult canGift)
        {
            if (!canGift.CanGift)
            {
                return new FailedGifting(canGift.Message);
            }

            if (!_playerProvider.TryGetPlayer(playerName, playerTeam, out var receivingPlayer))
            {
                return new FailedGifting($"Could not find a player named {playerName} on team {playerTeam}");
            }

            var senderSlot = _playerProvider.CurrentPlayerSlot;
            var senderTeam = _playerProvider.CurrentPlayerTeam;

            var receiverSlot = receivingPlayer.Slot;
            var gift = new Gift(item.Name, item.Amount, item.Value, traits, senderSlot, receiverSlot, senderTeam, playerTeam);
            return SendGift(gift);
        }

        public GiftingResult RefundGift(Gift gift)
        {
            if (gift.IsRefund)
            {
                return new FailedGifting("Cannot refund a gift that is already a refund");
            }

            gift.IsRefund = true;
            return SendGift(gift);
        }

        private GiftingResult SendGift(Gift gift)
        {
            try
            {
                var targetPlayer = gift.IsRefund
                    ? _playerProvider.GetPlayer(gift.SenderSlot, gift.SenderTeam)
                    : _playerProvider.GetPlayer(gift.ReceiverSlot, gift.ReceiverTeam);

                var motherBox = GetMotherbox(targetPlayer.Team);
                var giftBoxMetadata = motherBox[targetPlayer.Slot];
                var giftBoxVersion = giftBoxMetadata.MaximumGiftDataVersion;
                if (giftBoxVersion < DataVersion.FirstVersion)
                {
                    giftBoxVersion = DataVersion.FirstVersion;
                }

                var giftBoxKey = _keyProvider.GetGiftBoxDataStorageKey(targetPlayer.Team, targetPlayer.Slot);

                CreateGiftBoxIfNeeded(giftBoxKey);
                var newGiftEntry = _currentGiftConverter.CreateDataStorageUpdateEntry(gift.ID, gift, giftBoxVersion);
                _session.DataStorage[Scope.Global, giftBoxKey] += Operation.Update(newGiftEntry);
                return new SuccessfulGifting(gift.ID);
            }
            catch (Exception ex)
            {
                return new FailedGifting(gift.ID, ex.Message);
            }
        }

        public Dictionary<string, Gift> GetAllGiftsAndEmptyGiftBox()
        {
            var gifts = CheckGiftBox();
            RemoveGiftsFromGiftBox(gifts.Keys);
            return gifts;
        }

        public async Task<Dictionary<string, Gift>> GetAllGiftsAndEmptyGiftBoxAsync()
        {
            var gifts = await CheckGiftBoxAsync();
            RemoveGiftsFromGiftBox(gifts.Keys);
            return gifts;
        }

        public Dictionary<string, Gift> CheckGiftBox()
        {
            var giftBoxKey = _keyProvider.GetGiftBoxDataStorageKey(_playerProvider.CurrentPlayerTeam, _playerProvider.CurrentPlayerSlot);
            var gifts = GetGiftBoxContent(giftBoxKey);
            return gifts;
        }

        public async Task<Dictionary<string, Gift>> CheckGiftBoxAsync()
        {
            var giftBoxKey = _keyProvider.GetGiftBoxDataStorageKey(_playerProvider.CurrentPlayerTeam, _playerProvider.CurrentPlayerSlot);
            var gifts = await GetGiftBoxContentAsync(giftBoxKey);
            return gifts;
        }

        private void EmptyGiftBox()
        {
            GetAllGiftsAndEmptyGiftBox();
        }

        public void RemoveGiftsFromGiftBox(IEnumerable<string> giftsIds)
        {
            foreach (var giftId in giftsIds)
            {
                RemoveGiftFromGiftBox(giftId);
            }
        }

        public void RemoveGiftFromGiftBox(string giftId)
        {
            var giftBoxKey = _keyProvider.GetGiftBoxDataStorageKey(_playerProvider.CurrentPlayerTeam, _playerProvider.CurrentPlayerSlot);
            _session.DataStorage[Scope.Global, giftBoxKey] += Operation.Pop(giftId);
        }

        private Dictionary<string, Gift> GetGiftBoxContent(string giftBoxKey)
        {
            CreateGiftBoxIfNeeded(giftBoxKey);
            var existingGiftBox = _session.DataStorage[Scope.Global, giftBoxKey];
            var gifts = _currentGiftConverter.ReadFromDataStorage(existingGiftBox);
            return gifts;
        }

        private async Task<Dictionary<string, Gift>> GetGiftBoxContentAsync(string giftBoxKey)
        {
            CreateGiftBoxIfNeeded(giftBoxKey);
            var existingGiftBox = _session.DataStorage[Scope.Global, giftBoxKey];
            var gifts = await existingGiftBox.GetAsync();
            return _currentGiftConverter.ReadFromDataStorage(gifts);
        }

        public CanGiftResult CanGiftToPlayer(string playerName)
        {
            return CanGiftToPlayer(playerName, _playerProvider.CurrentPlayerTeam);
        }

        public async Task<CanGiftResult> CanGiftToPlayerAsync(string playerName)
        {
            return await CanGiftToPlayerAsync(playerName, _playerProvider.CurrentPlayerTeam);
        }

        public CanGiftResult CanGiftToPlayer(string playerName, int playerTeam)
        {
            return CanGiftToPlayer(playerName, playerTeam, Enumerable.Empty<string>());
        }

        public async Task<CanGiftResult> CanGiftToPlayerAsync(string playerName, int playerTeam)
        {
            return await CanGiftToPlayerAsync(playerName, playerTeam, Enumerable.Empty<string>());
        }

        public CanGiftResult CanGiftToPlayer(string playerName, IEnumerable<string> giftTraits)
        {
            return CanGiftToPlayer(playerName, _playerProvider.CurrentPlayerTeam, giftTraits);
        }

        public async Task<CanGiftResult> CanGiftToPlayerAsync(string playerName, IEnumerable<string> giftTraits)
        {
            return await CanGiftToPlayerAsync(playerName, _playerProvider.CurrentPlayerTeam, giftTraits);
        }

        public CanGiftResult CanGiftToPlayer(string playerName, int playerTeam, IEnumerable<string> giftTraits)
        {
            if (!_playerProvider.TryGetPlayer(playerName, out var player))
            {
                return new CanGiftResult($"Could not find a player named {playerName}");
            }

            return CanGiftToPlayer(player.Slot, playerTeam, giftTraits);
        }

        public async Task<CanGiftResult> CanGiftToPlayerAsync(string playerName, int playerTeam, IEnumerable<string> giftTraits)
        {
            if (!_playerProvider.TryGetPlayer(playerName, out var player))
            {
                return new CanGiftResult($"Could not find a player named {playerName}");
            }

            return await CanGiftToPlayerAsync(player.Slot, playerTeam, giftTraits);
        }

        public CanGiftResult CanGiftToPlayer(int playerSlot)
        {
            return CanGiftToPlayer(playerSlot, _playerProvider.CurrentPlayerTeam);
        }

        public async Task<CanGiftResult> CanGiftToPlayerAsync(int playerSlot)
        {
            return await CanGiftToPlayerAsync(playerSlot, _playerProvider.CurrentPlayerTeam);
        }

        public CanGiftResult CanGiftToPlayer(int playerSlot, int playerTeam)
        {
            return CanGiftToPlayer(playerSlot, playerTeam, Enumerable.Empty<string>());
        }

        public async Task<CanGiftResult> CanGiftToPlayerAsync(int playerSlot, int playerTeam)
        {
            return await CanGiftToPlayerAsync(playerSlot, playerTeam, Enumerable.Empty<string>());
        }

        public CanGiftResult CanGiftToPlayer(int playerSlot, IEnumerable<string> giftTraits)
        {
            return CanGiftToPlayer(playerSlot, _playerProvider.CurrentPlayerTeam, giftTraits);
        }

        public async Task<CanGiftResult> CanGiftToPlayerAsync(int playerSlot, IEnumerable<string> giftTraits)
        {
            return await CanGiftToPlayerAsync(playerSlot, _playerProvider.CurrentPlayerTeam, giftTraits);
        }

        public CanGiftResult CanGiftToPlayer(int playerSlot, int playerTeam, IEnumerable<string> giftTraits)
        {
            var motherBox = GetMotherbox(playerTeam);
            return CanGiftToPlayer(playerSlot, playerTeam, giftTraits, motherBox);
        }

        public async Task<CanGiftResult> CanGiftToPlayerAsync(int playerSlot, int playerTeam, IEnumerable<string> giftTraits)
        {
            var motherBox = await GetMotherboxAsync(playerTeam);
            return CanGiftToPlayer(playerSlot, playerTeam, giftTraits, motherBox);
        }

        private CanGiftResult CanGiftToPlayer(int playerSlot, int playerTeam, IEnumerable<string> giftTraits, Dictionary<int, GiftBox> motherBox)
        {
            if (!motherBox.ContainsKey(playerSlot))
            {
                return new CanGiftResult("Player does not exist or does not have a giftbox");
            }

            var giftBox = motherBox[playerSlot];
            if (!giftBox.IsOpen)
            {
                return new CanGiftResult("This giftbox is currently closed");
            }

            if (giftBox.MinimumGiftDataVersion > DataVersion.Current)
            {
                return new CanGiftResult($"This player can only receive gifts from a more recent data version. Your client should update to gifting Data Version {giftBox.MinimumGiftDataVersion}. If you are not the developer, you should contact them to tell them about this.");
            }

            var owner = _playerProvider.GetPlayer(playerSlot, playerTeam);
            if (giftBox.AcceptsAnyGift)
            {
                return new CanGiftResult();
            }

            if (!giftBox.DesiredTraits.Any(trait => giftTraits.Contains(trait, StringComparer.OrdinalIgnoreCase)))
            {
                return new CanGiftResult($"This player cannot accept this gift. They can only accept the following traits: [{string.Join(",", giftBox.DesiredTraits)}]");
            }

            return new CanGiftResult();
        }

        private Dictionary<int, GiftBox> GetMotherbox(int playerTeam)
        {
            var motherboxKey = _keyProvider.GetMotherBoxDataStorageKey(playerTeam);
            var motherBox = _currentGiftBoxConverter.ReadFromDataStorage(_session.DataStorage[Scope.Global, motherboxKey]);
            return motherBox;
        }

        private async Task<Dictionary<int, GiftBox>> GetMotherboxAsync(int playerTeam)
        {
            var motherboxKey = _keyProvider.GetMotherBoxDataStorageKey(playerTeam);
            var motherBoxToken = await _session.DataStorage[Scope.Global, motherboxKey].GetAsync();
            var motherBox = _currentGiftBoxConverter.ReadFromDataStorage(motherBoxToken);
            return motherBox;
        }

        private void CreateMotherboxIfNeeded(string motherboxKey)
        {
            _session.DataStorage[Scope.Global, motherboxKey].Initialize(EmptyMotherboxDictionary);
        }

        private void CreateGiftBoxIfNeeded(string giftBoxKey)
        {
            _session.DataStorage[Scope.Global, giftBoxKey].Initialize(EmptyGiftDictionary);
        }

        public delegate void GiftReceivedHandler(Gift newGift);
        private GiftReceivedHandler _giftReceivedHandler;

        public event GiftReceivedHandler OnNewGift
        {
            add
            {
                if (_giftReceivedHandler == null)
                {
                    var dataStorageKey = _keyProvider.GetGiftBoxDataStorageKey();
                    _session.DataStorage[dataStorageKey].OnValueChanged += GiftListener;
                }

                _giftReceivedHandler += value;
            }
            remove
            {
                _giftReceivedHandler -= value;
                if (_giftReceivedHandler == null)
                {
                    var dataStorageKey = _keyProvider.GetGiftBoxDataStorageKey();
                    _session.DataStorage[dataStorageKey].OnValueChanged -= GiftListener;
                }
            }
        }

        private void GiftListener(JToken originalValue, JToken newValue, Dictionary<string, JToken> additionalArguments)
        {
            var oldGifts = _currentGiftConverter.ReadFromDataStorage(originalValue);
            var newGifts = _currentGiftConverter.ReadFromDataStorage(newValue);
            foreach (var key in oldGifts.Keys)
            {
                newGifts.Remove(key);
            }

            // Process and parse gifts
            foreach (var gift in newGifts.Values)
            {
                _giftReceivedHandler?.Invoke(gift);
            }
        }

        [Obsolete("SubscribeToNewGifts is deprecated. Instead, use the event OnNewGift and subscribe by adding the handlers you need")]
        public void SubscribeToNewGifts(Action<Dictionary<string, Gift>> newGiftsCallback)
        {
            var dataStorageKey = _keyProvider.GetGiftBoxDataStorageKey();
            _session.DataStorage[Scope.Global, dataStorageKey].OnValueChanged += (originalValue, newValue, additionalArguments) => OnNewGiftObsolete(originalValue, newValue, newGiftsCallback);
        }

        [Obsolete("Obsolete private event used by SubscribeToNewGifts")]
        private void OnNewGiftObsolete(JToken originalValue, JToken newValue, Action<Dictionary<string, Gift>> newGiftsCallback)
        {
            var newGifts = _currentGiftConverter.ReadFromDataStorage(newValue);
            if (newGifts.Any())
            {
                newGiftsCallback(newGifts);
            }
        }

        #region Obsolete stuff

        [Obsolete("The overloads with out parameters are now deprecated, please use the overloads that return a GiftingResult instead")]
        public bool SendGift(GiftItem item, string playerName, out string giftId)
        {
            var result = SendGift(item, playerName);
            giftId = result.GiftId;
            return result.Success;
        }

        [Obsolete("The overloads with out parameters are now deprecated, please use the overloads that return a GiftingResult instead")]
        public bool SendGift(GiftItem item, string playerName, int playerTeam, out string giftId)
        {
            var result = SendGift(item, playerName, playerTeam);
            giftId = result.GiftId;
            return result.Success;
        }

        [Obsolete("The overloads with out parameters are now deprecated, please use the overloads that return a GiftingResult instead")]
        public bool SendGift(GiftItem item, GiftTrait[] traits, string playerName, out string giftId)
        {
            var result = SendGift(item, traits, playerName);
            giftId = result.GiftId;
            return result.Success;
        }

        [Obsolete("The overloads with out parameters are now deprecated, please use the overloads that return a GiftingResult instead")]
        public bool SendGift(GiftItem item, GiftTrait[] traits, string playerName, int playerTeam, out string giftId)
        {
            var result = SendGift(item, traits, playerName, playerTeam);
            giftId = result.GiftId;
            return result.Success;
        }

        #endregion Obsolete stuff
    }
}
