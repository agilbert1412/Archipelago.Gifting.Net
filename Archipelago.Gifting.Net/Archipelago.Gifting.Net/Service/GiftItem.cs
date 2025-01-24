using System.Numerics;

namespace Archipelago.Gifting.Net.Versioning.Gifts
{
    public class GiftItem
    {
        public string Name { get; set; }
        public int Amount { get; set; }

        /// <summary>
        /// This is the value per item, in the standard Archipelago currency.
        /// This is not the value of the whole gift. The value of the gift is the value of the item, multiplied by the amount of the item
        /// </summary>
        public BigInteger Value { get; set; }

        public GiftItem(string name, int amount, BigInteger value)
        {
            Name = name;
            Amount = amount;
            Value = value;
        }
    }
}
