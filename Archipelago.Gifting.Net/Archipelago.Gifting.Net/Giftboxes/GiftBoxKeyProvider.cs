using Archipelago.Gifting.Net.Utilities;
using Archipelago.MultiClient.Net;

namespace Archipelago.Gifting.Net.Giftboxes
{
    internal class GiftBoxKeyProvider
    {
        public const string MOTHERBOX_KEY_FORMAT = "GiftBoxes;{0}";
        public const string GIFTBOX_KEY_FORMAT = "GiftBox;{0};{1}";

        private ArchipelagoSession _session;
        private PlayerProvider _playerProvider;

        public GiftBoxKeyProvider(ArchipelagoSession session, PlayerProvider playerProvider)
        {
            _session = session;
            _playerProvider = playerProvider;
        }

        public string GetMotherBoxDataStorageKey()
        {
            return GetMotherBoxDataStorageKey(_playerProvider.CurrentPlayerTeam);
        }

        public string GetMotherBoxDataStorageKey(int playerTeam)
        {
            return string.Format(MOTHERBOX_KEY_FORMAT, playerTeam);
        }

        public string GetGiftBoxDataStorageKey()
        {
            return GetGiftBoxDataStorageKey(_playerProvider.CurrentPlayerSlot);
        }

        public string GetGiftBoxDataStorageKey(int playerSlot)
        {
            return GetGiftBoxDataStorageKey(_playerProvider.CurrentPlayerTeam, playerSlot);
        }

        public string GetGiftBoxDataStorageKey(int playerTeam, int playerSlot)
        {
            return string.Format(GIFTBOX_KEY_FORMAT, playerTeam, playerSlot);
        }
    }
}
