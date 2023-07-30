using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archipelago.Gifting.Net
{
    public class GiftFlag
    {
        public const string None = "None";
        public const string Food = "Food";
        public const string Drink = "Drink";
        public const string Consumable = "Consumable";
        public const string Speed = "Speed";
        public const string Fish = "Fish";
        public const string Heal = "Heal";
        public const string Mana = "Mana";
        public const string Trap = "Trap";
        public const string Buff = "Buff";
        public const string Life = "Life";
        public const string Weapon = "Weapon";
        public const string Armor = "Armor";
        public const string Tool = "Tool";
        public const string Cure = "Cure";

        public static readonly string[] AllFlags = new[]
        {
            None, Food, Drink, Consumable, Speed, Fish, Heal, Mana, Trap, Buff, Life, Weapon, Armor, Tool, Cure,
        };
    }
}
