namespace Archipelago.Gifting.Net.Gifts.Versions
{
    internal static class DataVersion
    {
        public const int GIFT_DATA_VERSION_1 = 1;
        public const int GIFT_DATA_VERSION_2 = 2;
        public const int GIFT_DATA_VERSION_3 = 3;

        public static int FirstVersion => GIFT_DATA_VERSION_1;
        public static int Current => GIFT_DATA_VERSION_3;
    }
}
