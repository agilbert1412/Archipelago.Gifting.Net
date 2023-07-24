using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json.Linq;

namespace Archipelago.Gifting.Net
{
    public class GiftingService : IGiftingService
    {
        private ArchipelagoSession _session;
        private PlayerInfo CurrentPlayer => GetPlayer(_session.ConnectionInfo.Slot);
        private string CurrentPlayerName => CurrentPlayer.Name;

        public GiftingService(ArchipelagoSession session)
        {
            _session = session;
        }

        public void OpenGiftBox()
        {
            var giftboxKey = GetGiftboxDataStorageKey();

            if (GiftboxExists(giftboxKey))
            {
                return;
            }

            var gifts = GetGiftboxContent(giftboxKey);
            if (gifts != null)
            {
                return;
            }

            _session.DataStorage[Scope.Global, giftboxKey] = Array.Empty<Gift>();
        }

        public void CloseGiftBox()
        {
            var giftboxKey = GetGiftboxDataStorageKey();
            _session.DataStorage[Scope.Global, giftboxKey] = null;
        }

        public bool SendGift(GiftItem item, string playerName)
        {
            return SendGift(item, Array.Empty<GiftTrait>(), playerName);
        }

        public bool SendGift(GiftItem item, GiftTrait[] traits, string playerName)
        {
            return SendGift(item, traits, playerName, _session.ConnectionInfo.Team);
        }

        public bool SendGift(GiftItem item, string playerName, int playerTeam)
        {
            return SendGift(item, Array.Empty<GiftTrait>(), playerName, playerTeam);
        }

        public bool SendGift(GiftItem item, GiftTrait[] traits, string playerName, int playerTeam)
        {
            var sendingPlayer = CurrentPlayer;
            var receivingPlayer = GetPlayer(playerName, playerTeam);
            var gift = new Gift(item, traits, sendingPlayer.Name, receivingPlayer.Name);
            return SendGift(gift);
        }

        public bool RefundGift(Gift gift)
        {
            gift.IsRefund = true;
            return SendGift(gift);
        }

        private bool SendGift(Gift gift)
        {
            var giftboxKey = GetGiftboxDataStorageKey(gift.IsRefund ? gift.Sender : gift.Receiver);

            if (!GiftboxExists(giftboxKey))
            {
                return false;
            }

            var giftBoxContent = GetGiftboxContent(giftboxKey);
            var newGiftBoxContent = new List<Gift>();
            if (giftBoxContent != null)
            {
                newGiftBoxContent.AddRange(giftBoxContent);
            }
            newGiftBoxContent.Add(gift);
            SetGiftboxContent(giftboxKey, newGiftBoxContent);
            SendGiftNotification(gift);
            return true;
        }

        public Gift[]? GetAllGiftsAndEmptyGiftbox()
        {
            var gifts = CheckGiftBox();
            if (gifts == null)
            {
                return null;
            }
            EmptyGiftBox();
            return gifts;
        }

        public Gift[]? CheckGiftBox()
        {
            var giftboxKey = GetGiftboxDataStorageKey();
            var gifts = GetGiftboxContent(giftboxKey);
            return gifts;
        }

        public void EmptyGiftBox()
        {
            var giftboxKey = GetGiftboxDataStorageKey();
            _session.DataStorage[Scope.Global, giftboxKey] = Array.Empty<Gift>();
        }

        private string GetGiftboxDataStorageKey()
        {
            return GetGiftboxDataStorageKey(CurrentPlayerName);
        }

        private string GetGiftboxDataStorageKey(string playerName)
        {
            var giftbox = new GiftBox(playerName);
            var giftboxKey = giftbox.GetDataStorageKey();
            return giftboxKey;
        }

        private Gift[]? GetGiftboxContent(string giftboxKey)
        {
            var existingGiftBox = _session.DataStorage[Scope.Global, giftboxKey];

            var gifts = existingGiftBox?.To<Gift[]>();
            return gifts;
        }

        public bool CanGiftToPlayer(string playerName)
        {
            var giftbox = new GiftBox(playerName);
            var giftboxKey = giftbox.GetDataStorageKey();
            return GiftboxExists(giftboxKey);
        }

        private bool GiftboxExists(string giftboxKey)
        {
            var content = GetGiftboxContent(giftboxKey);
            return content != null;
        }

        private void SetGiftboxContent(string giftboxKey, IEnumerable<Gift> content)
        {
            _session.DataStorage[Scope.Global, giftboxKey] = content.ToArray();
        }

        private void SendGiftNotification(Gift gift)
        {
            var packet = new BouncePacket()
            {
                Tags = new List<string> { "Gift" },
                Data = new Dictionary<string, JToken>
                {
                    { "ReceiverName", gift.Receiver },
                    { "SenderName", gift.Sender },
                    { "IsRefund", gift.IsRefund },
                },
            };

            _session.Socket.SendPacketAsync(packet);
        }

        private PlayerInfo GetPlayer(string playerName)
        {
            return GetPlayer(playerName, _session.ConnectionInfo.Team);
        }

        private PlayerInfo GetPlayer(string playerName, int playerTeam)
        {
            return _session.Players.Players[playerTeam].First(player => player.Name == playerName || player.Alias == playerName);
        }

        private PlayerInfo GetPlayer(int playerSlot)
        {
            return GetPlayer(playerSlot, _session.ConnectionInfo.Team);
        }

        private PlayerInfo GetPlayer(int playerSlot, int playerTeam)
        {
            return _session.Players.Players[playerTeam].First(player => player.Slot == playerSlot);
        }
    }
}
