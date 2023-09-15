using Archipelago.Gifting.Net.Gifts;
using Archipelago.Gifting.Net.Gifts.Versions;
using Archipelago.Gifting.Net.Traits;
using Archipelago.Gifting.Net.Utilities;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Archipelago.Gifting.Net.Giftboxes;
using Archipelago.Gifting.Net.Gifts.Versions.Current;

namespace Archipelago.Gifting.Net.Service
{
    public class GiftingService : IGiftingService
    {
        private ArchipelagoSession _session;
        private PlayerProvider _playerProvider;
        private GiftBoxKeyProvider _keyProvider;
        private Converter _currentConverter;

        private JToken EmptyMotherboxDictionary => JToken.FromObject(new Dictionary<int, GiftBox>());
        private JToken EmptyGiftDictionary => JToken.FromObject(new Dictionary<string, Gift>());

        public GiftingService(ArchipelagoSession session)
        {
            _session = session;
            _playerProvider = new PlayerProvider(_session);
            _keyProvider = new GiftBoxKeyProvider(_session, _playerProvider);
            _currentConverter = new Converter(_playerProvider);
            var motherboxKey = _keyProvider.GetMotherBoxDataStorageKey();
            CreateMotherboxIfNeeded(motherboxKey);
        }

        public string GetMyGiftBoxKey()
        {
            return _keyProvider.GetGiftBoxDataStorageKey();
        }

        /// <summary>
        /// Open a giftbox that can accept any gift with no trait preference
        /// </summary>
        public void OpenGiftBox()
        {
            UpdateGiftBox(new GiftBox(true));
        }

        /// <summary>
        /// Open a giftbox with custom acceptance preference
        /// </summary>
        /// <param name="acceptAnyGift">Whether this giftbox can accept any gift, or only gifts with the specified traits</param>
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

        internal void UpdateGiftBox(GiftBox entry)
        {
            var motherboxKey = _keyProvider.GetMotherBoxDataStorageKey();
            var myGiftBoxEntry = new Dictionary<int, GiftBox>
            {
                {_playerProvider.CurrentPlayerSlot, entry},
            };
            _session.DataStorage[Scope.Global, motherboxKey] += Operation.Update(myGiftBoxEntry);
        }

        public bool SendGift(GiftItem item, string playerName)
        {
            return SendGift(item, playerName, out _);
        }

        public bool SendGift(GiftItem item, string playerName, out string giftId)
        {
            return SendGift(item, new GiftTrait[0], playerName, out giftId);
        }

        public bool SendGift(GiftItem item, GiftTrait[] traits, string playerName)
        {
            return SendGift(item, traits, playerName, out _);
        }

        public bool SendGift(GiftItem item, GiftTrait[] traits, string playerName, out string giftId)
        {
            return SendGift(item, traits, playerName, _session.ConnectionInfo.Team, out giftId);
        }

        public bool SendGift(GiftItem item, string playerName, int playerTeam)
        {
            return SendGift(item, playerName, playerTeam, out _);
        }

        public bool SendGift(GiftItem item, string playerName, int playerTeam, out string giftId)
        {
            return SendGift(item, new GiftTrait[0], playerName, playerTeam, out giftId);
        }

        public bool SendGift(GiftItem item, GiftTrait[] traits, string playerName, int playerTeam)
        {
            return SendGift(item, traits, playerName, playerTeam, out _);
        }

        public bool SendGift(GiftItem item, GiftTrait[] traits, string playerName, int playerTeam, out string giftId)
        {
            var canGift = CanGiftToPlayer(playerName, playerTeam, traits.Select(x => x.Trait));
            return SendGift(item, traits, playerName, playerTeam, canGift, out giftId);
        }

        public async Task<bool> SendGiftAsync(GiftItem item, GiftTrait[] traits, string playerName, int playerTeam)
        {
            var canGift = await CanGiftToPlayerAsync(playerName, playerTeam, traits.Select(x => x.Trait));
            return SendGift(item, traits, playerName, playerTeam, canGift, out _);
        }

        private bool SendGift(GiftItem item, GiftTrait[] traits, string playerName, int playerTeam, bool canGift, out string giftId)
        {
            if (!canGift)
            {
                giftId = string.Empty;
                return false;
            }

            if (!_playerProvider.TryGetPlayer(playerName, playerTeam, out var receivingPlayer))
            {
                giftId = string.Empty;
                return false;
            }

            var senderSlot = _playerProvider.CurrentPlayerSlot;
            var senderTeam = _playerProvider.CurrentPlayerTeam;

            var receiverSlot = receivingPlayer.Slot;
            var gift = new Gift(item.Name, item.Amount, item.Value, traits, senderSlot, receiverSlot, senderTeam, playerTeam);
            giftId = gift.ID;
            return SendGift(gift);
        }

        public bool RefundGift(Gift gift)
        {
            if (gift.IsRefund)
            {
                return false;
            }

            gift.IsRefund = true;
            return SendGift(gift);
        }

        private bool SendGift(Gift gift)
        {
            try
            {
                var targetPlayer = gift.IsRefund
                    ? _playerProvider.GetPlayer(gift.SenderSlot, gift.SenderTeam)
                    : _playerProvider.GetPlayer(gift.ReceiverSlot, gift.ReceiverTeam);
                var motherboxKey = _keyProvider.GetMotherBoxDataStorageKey(targetPlayer.Team);
                var motherBox = _session.DataStorage[Scope.Global, motherboxKey].To<Dictionary<int, GiftBox>>();
                var giftboxMetadata = motherBox[targetPlayer.Slot];
                var giftboxVersion = giftboxMetadata.MaximumGiftDataVersion;
                if (giftboxVersion < DataVersion.FirstVersion)
                {
                    giftboxVersion = DataVersion.FirstVersion;
                }

                var giftboxKey = _keyProvider.GetGiftBoxDataStorageKey(targetPlayer.Team, targetPlayer.Slot);

                CreateGiftboxIfNeeded(giftboxKey);
                var newGiftEntry = _currentConverter.CreateDataStorageUpdateEntry(gift, giftboxVersion);
                _session.DataStorage[Scope.Global, giftboxKey] += Operation.Update(newGiftEntry);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Dictionary<string, Gift> GetAllGiftsAndEmptyGiftbox()
        {
            var gifts = CheckGiftBox();
            RemoveGiftsFromGiftBox(gifts.Keys);
            return gifts;
        }

        public async Task<Dictionary<string, Gift>> GetAllGiftsAndEmptyGiftboxAsync()
        {
            var gifts = await CheckGiftBoxAsync();
            RemoveGiftsFromGiftBox(gifts.Keys);
            return gifts;
        }

        public Dictionary<string, Gift> CheckGiftBox()
        {
            var giftboxKey = _keyProvider.GetGiftBoxDataStorageKey(_playerProvider.CurrentPlayerTeam, _playerProvider.CurrentPlayerSlot);
            var gifts = GetGiftboxContent(giftboxKey);
            return gifts;
        }

        public async Task<Dictionary<string, Gift>> CheckGiftBoxAsync()
        {
            var giftboxKey = _keyProvider.GetGiftBoxDataStorageKey(_playerProvider.CurrentPlayerTeam, _playerProvider.CurrentPlayerSlot);
            var gifts = await GetGiftboxContentAsync(giftboxKey);
            return gifts;
        }

        private void EmptyGiftBox()
        {
            GetAllGiftsAndEmptyGiftbox();
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
            var giftboxKey = _keyProvider.GetGiftBoxDataStorageKey(_playerProvider.CurrentPlayerTeam, _playerProvider.CurrentPlayerSlot);
            _session.DataStorage[Scope.Global, giftboxKey] += Operation.Pop(giftId);
        }

        private Dictionary<string, Gift> GetGiftboxContent(string giftboxKey)
        {
            CreateGiftboxIfNeeded(giftboxKey);
            var existingGiftBox = _session.DataStorage[Scope.Global, giftboxKey];
            var gifts = _currentConverter.ReadFromDataStorage(existingGiftBox);
            return gifts;
        }

        private async Task<Dictionary<string, Gift>> GetGiftboxContentAsync(string giftboxKey)
        {
            CreateGiftboxIfNeeded(giftboxKey);
            var existingGiftBox = _session.DataStorage[Scope.Global, giftboxKey];
            var gifts = await existingGiftBox.GetAsync();
            return _currentConverter.ReadFromDataStorage(gifts);
        }

        public bool CanGiftToPlayer(string playerName)
        {
            return CanGiftToPlayer(playerName, _playerProvider.CurrentPlayerTeam);
        }

        public bool CanGiftToPlayer(string playerName, int playerTeam)
        {
            return CanGiftToPlayer(playerName, playerTeam, Enumerable.Empty<string>());
        }

        public bool CanGiftToPlayer(string playerName, IEnumerable<string> giftTraits)
        {
            return CanGiftToPlayer(playerName, _playerProvider.CurrentPlayerTeam, giftTraits);
        }

        public bool CanGiftToPlayer(string playerName, int playerTeam, IEnumerable<string> giftTraits)
        {
            if (!_playerProvider.TryGetPlayer(playerName, out var player))
            {
                return false;
            }
            return CanGiftToPlayer(player.Slot, playerTeam, giftTraits);
        }

        public async Task<bool> CanGiftToPlayerAsync(string playerName, int playerTeam, IEnumerable<string> giftTraits)
        {
            if (!_playerProvider.TryGetPlayer(playerName, out var player))
            {
                return false;
            }

            return await CanGiftToPlayerAsync(player.Slot, playerTeam, giftTraits);
        }

        public bool CanGiftToPlayer(int playerSlot)
        {
            return CanGiftToPlayer(playerSlot, _playerProvider.CurrentPlayerTeam);
        }

        public bool CanGiftToPlayer(int playerSlot, int playerTeam)
        {
            return CanGiftToPlayer(playerSlot, playerTeam, Enumerable.Empty<string>());
        }

        public bool CanGiftToPlayer(int playerSlot, IEnumerable<string> giftTraits)
        {
            return CanGiftToPlayer(playerSlot, _playerProvider.CurrentPlayerTeam, giftTraits);
        }

        public bool CanGiftToPlayer(int playerSlot, int playerTeam, IEnumerable<string> giftTraits)
        {
            var motherboxKey = _keyProvider.GetMotherBoxDataStorageKey(playerTeam);
            var motherBox = _session.DataStorage[Scope.Global, motherboxKey].To<Dictionary<int, GiftBox>>();
            return CanGiftToPlayer(playerSlot, playerTeam, giftTraits, motherBox);
        }

        public async Task<bool> CanGiftToPlayerAsync(int playerSlot, int playerTeam, IEnumerable<string> giftTraits)
        {
            var motherboxKey = _keyProvider.GetMotherBoxDataStorageKey(playerTeam);
            var motherBoxToken = await _session.DataStorage[Scope.Global, motherboxKey].GetAsync();
            var motherBox = motherBoxToken.ToObject<Dictionary<int, GiftBox>>();
            return CanGiftToPlayer(playerSlot, playerTeam, giftTraits, motherBox);
        }

        private bool CanGiftToPlayer(int playerSlot, int playerTeam, IEnumerable<string> giftTraits, Dictionary<int, GiftBox> motherBox)
        {
            if (!motherBox.ContainsKey(playerSlot))
            {
                return false;
            }

            var giftBox = motherBox[playerSlot];
            if (!giftBox.IsOpen || giftBox.MinimumGiftDataVersion > DataVersion.Current)
            {
                return false;
            }

            var owner = _playerProvider.GetPlayer(playerSlot, playerTeam);
            if (giftBox.AcceptsAnyGift)
            {
                return true;
            }

            return giftBox.DesiredTraits.Any(trait => giftTraits.Contains(trait, StringComparer.OrdinalIgnoreCase));
        }

        private void CreateMotherboxIfNeeded(string motherboxKey)
        {
            _session.DataStorage[Scope.Global, motherboxKey].Initialize(EmptyMotherboxDictionary);
        }

        private void CreateGiftboxIfNeeded(string giftBoxKey)
        {
            _session.DataStorage[Scope.Global, giftBoxKey].Initialize(EmptyGiftDictionary);
        }

        public void SubscribeToNewGifts(Action<Dictionary<string, Gift>> newGiftsCallback)
        {
            var dataStorageKey = _keyProvider.GetGiftBoxDataStorageKey();
            // _session.DataStorage[Scope.Global, dataStorageKey].OnValueChanged += (originalValue, newValue) => OnNewGift(originalValue, newValue, newGiftsCallback);
            var notifyPacker = new SetNotifyPacket()
            {
                Keys = new[] { dataStorageKey },
            };
            _session.Socket.SendPacket(notifyPacker);
            _session.Socket.PacketReceived += (packet) => OnNewGift(packet, newGiftsCallback);
        }

        private void OnNewGift(ArchipelagoPacketBase packet, Action<Dictionary<string, Gift>> newGiftsCallback)
        {
            if (!(packet is SetReplyPacket replyPacket))
            {
                return;
            }

            var gifts = _currentConverter.ReadFromDataStorage(replyPacket.Value);
            if (gifts == null || !gifts.Any())
            {
                return;
            }

            newGiftsCallback(gifts);
        }

        private void OnNewGift(JToken originalValue, JToken newValue, Action newGiftsCallback)
        {
            var newGifts = newValue.ToObject<Dictionary<string, Gift>>();
            if (newGifts.Any())
            {
                newGiftsCallback();
            }
        }
    }
}
