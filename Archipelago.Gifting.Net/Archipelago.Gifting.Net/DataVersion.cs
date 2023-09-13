namespace Archipelago.Gifting.Net
{
    public static class DataVersion
    {
        public const int GIFT_DATA_VERSION_1 = 1;
        public const int GIFT_DATA_VERSION_2 = 2;

        public static int FirstVersion => GIFT_DATA_VERSION_1;
        public static int Current => GIFT_DATA_VERSION_2;
    }
}
