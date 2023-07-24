using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Archipelago.Gifting.Net
{
    public class GiftItem
    {
        public string Name { get; set; }
        public int Amount { get; set; }
        public BigInteger Value { get; set; }

        public GiftItem(string name, int amount, BigInteger value)
        {
            Name = name;
            Amount = amount;
            Value = value;
        }
    }
}
