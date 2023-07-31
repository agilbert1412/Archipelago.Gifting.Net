using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json.Linq;

namespace Archipelago.Gifting.Net
{
    public class GiftingService : IGiftingService
    {
        private ArchipelagoSession _session;
        private PlayerProvider _playerProvider;
        private GiftBoxKeyProvider _keyProvider;

        private JToken EmptyMotherboxDictionary => JToken.FromObject(new Dictionary<int, GiftBox>());
        private JToken EmptyGiftDictionary => JToken.FromObject(new Dictionary<Guid, Gift>());

        public GiftingService(ArchipelagoSession session)
        {
            _session = session;
            _playerProvider = new PlayerProvider(_session);
            _keyProvider = new GiftBoxKeyProvider(_session, _playerProvider);
        }

        /// <summary>
        /// Open a giftbox that can accept any gift with no trait preference
        /// </summary>
        public void OpenGiftBox()
        {
            UpdateGiftBox(new GiftBox(_playerProvider.CurrentPlayerName, _playerProvider.CurrentPlayerGame, true));
        }

        /// <summary>
        /// Open a giftbox with custom acceptance preference
        /// </summary>
        /// <param name="acceptAnyGift">Whether this giftbox can accept any gift, or only gifts with the specified traits</param>
        /// <param name="desiredTraits">If can accept any gift, these are traits that make for a "good" gift. If not, these are the only accepted traits</param>
        public void OpenGiftBox(bool acceptAnyGift, string[] desiredTraits)
        {
            UpdateGiftBox(new GiftBox(_playerProvider.CurrentPlayerName, _playerProvider.CurrentPlayerGame, acceptAnyGift, desiredTraits));
        }

        public void CloseGiftBox()
        {
            UpdateGiftBox(new GiftBox(false));
            EmptyGiftBox();
        }

        private void UpdateGiftBox(GiftBox entry)
        {
            var motherboxKey = _keyProvider.GetMotherBoxDataStorageKey();
            CreateMotherboxIfNeeded(motherboxKey);
            var myGiftBoxEntry = new Dictionary<int, GiftBox>
            {
                {_playerProvider.CurrentPlayerSlot, entry}
            };
            _session.DataStorage[Scope.Global, motherboxKey] += Operation.Update(myGiftBoxEntry);
        }

        public bool SendGift(GiftItem item, string playerName)
        {
            return SendGift(item, playerName, out _);
        }

        public bool SendGift(GiftItem item, string playerName, out Guid giftId)
        {
            return SendGift(item, Array.Empty<GiftTrait>(), playerName, out giftId);
        }

        public bool SendGift(GiftItem item, GiftTrait[] traits, string playerName)
        {
            return SendGift(item, traits, playerName, out _);
        }

        public bool SendGift(GiftItem item, GiftTrait[] traits, string playerName, out Guid giftId)
        {
            return SendGift(item, traits, playerName, _session.ConnectionInfo.Team, out giftId);
        }

        public bool SendGift(GiftItem item, string playerName, int playerTeam)
        {
            return SendGift(item, playerName, playerTeam, out _);
        }

        public bool SendGift(GiftItem item, string playerName, int playerTeam, out Guid giftId)
        {
            return SendGift(item, Array.Empty<GiftTrait>(), playerName, playerTeam, out giftId);
        }

        public bool SendGift(GiftItem item, GiftTrait[] traits, string playerName, int playerTeam)
        {
            return SendGift(item, traits, playerName, playerTeam, out _);
        }

        public bool SendGift(GiftItem item, GiftTrait[] traits, string playerName, int playerTeam, out Guid giftId)
        {
            var canGift = CanGiftToPlayer(playerName, playerTeam, traits.Select(x => x.Trait));
            if (!canGift)
            {
                giftId = Guid.Empty;
                return false;
            }

            var sendingPlayerName = _playerProvider.CurrentPlayerName;
            var sendingPlayerTeam = _playerProvider.CurrentPlayerTeam;
            var receivingPlayerName = _playerProvider.GetPlayer(playerName, playerTeam).Name;
            var gift = new Gift(item, traits, sendingPlayerName, receivingPlayerName, sendingPlayerTeam, playerTeam);
            giftId = gift.ID;
            return SendGift(gift);
        }

        public bool RefundGift(Gift gift)
        {
            gift.IsRefund = true;
            return SendGift(gift);
        }

        private bool SendGift(Gift gift)
        {
            try
            {
                var targetPlayer = gift.IsRefund
                    ? _playerProvider.GetPlayer(gift.SenderName, gift.SenderTeam)
                    : _playerProvider.GetPlayer(gift.ReceiverName, gift.ReceiverTeam);
                var giftboxKey = _keyProvider.GetGiftBoxDataStorageKey(targetPlayer.Team, targetPlayer.Slot);

                CreateGiftboxIfNeeded(giftboxKey);
                var newGiftEntry = new Dictionary<Guid, Gift>
                {
                    { gift.ID, gift }
                };

                _session.DataStorage[Scope.Global, giftboxKey] += Operation.Update(newGiftEntry);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Dictionary<Guid, Gift> GetAllGiftsAndEmptyGiftbox()
        {
            var gifts = CheckGiftBox();
            RemoveGiftsFromGiftBox(gifts.Keys);
            return gifts;
        }

        public Dictionary<Guid, Gift> CheckGiftBox()
        {
            var giftboxKey = _keyProvider.GetGiftBoxDataStorageKey(_playerProvider.CurrentPlayerTeam, _playerProvider.CurrentPlayerSlot);
            var gifts = GetGiftboxContent(giftboxKey);
            return gifts;
        }

        private void EmptyGiftBox()
        {
            GetAllGiftsAndEmptyGiftbox();
        }

        public void RemoveGiftsFromGiftBox(IEnumerable<Guid> giftsIds)
        {
            foreach (var giftId in giftsIds)
            {
                RemoveGiftFromGiftBox(giftId);
            }
        }

        public void RemoveGiftFromGiftBox(Guid giftId)
        {
            var giftboxKey = _keyProvider.GetGiftBoxDataStorageKey(_playerProvider.CurrentPlayerTeam, _playerProvider.CurrentPlayerSlot);
            _session.DataStorage[Scope.Global, giftboxKey] += Operation.Pop(giftId);
        }

        private Dictionary<Guid, Gift> GetGiftboxContent(string giftboxKey)
        {
            CreateGiftboxIfNeeded(giftboxKey);
            var existingGiftBox = _session.DataStorage[Scope.Global, giftboxKey];
            var gifts = existingGiftBox.To<Dictionary<Guid, Gift>>();
            return gifts;
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
            return CanGiftToPlayer(_playerProvider.GetPlayer(playerName).Slot, playerTeam, giftTraits);
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
            CreateMotherboxIfNeeded(motherboxKey);
            var motherBox = _session.DataStorage[Scope.Global, motherboxKey].To<Dictionary<int, GiftBox>>();

            if (!motherBox.ContainsKey(playerSlot))
            {
                return false;
            }

            var giftBox = motherBox[playerSlot];
            if (!giftBox.IsOpen)
            {
                return false;
            }

            if (giftBox.AcceptsAnyGift || giftBox.Game == _playerProvider.CurrentPlayerGame)
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

        private void SetGiftboxContent(string giftboxKey, IEnumerable<Gift> content)
        {
            _session.DataStorage[Scope.Global, giftboxKey] = content.ToArray();
        }
    }
}
