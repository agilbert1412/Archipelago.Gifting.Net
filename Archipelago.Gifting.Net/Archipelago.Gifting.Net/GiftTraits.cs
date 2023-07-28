using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archipelago.Gifting.Net
{
    public class GiftTrait
    {
        public string Trait { get; set; }
        public double Strength { get; set; }
        public double Duration { get; set; }

        public GiftTrait(string trait, double duration, double strength)
        {
            Trait = trait;
            Duration = duration;
            Strength = strength;
        }
    }
}
