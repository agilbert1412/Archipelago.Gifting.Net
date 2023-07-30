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
        public double Quality { get; set; }
        public double Duration { get; set; }

        public GiftTrait(string trait, double duration, double quality)
        {
            Trait = trait;
            Quality = quality;
            Duration = duration;
        }
    }
}
