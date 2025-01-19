using System.Collections.Generic;

namespace Archipelago.Gifting.Net.Gifts.Versions.Version2
{
    internal class Validator
    {
        public Validator()
        {

        }

        public bool Validate(Dictionary<string, Gift> gifts, out IList<string> errors)
        {
            errors = new List<string>();
            if (gifts == null)
            {
                return false;
            }

            var allValid = true;
            foreach (var idAndGift in gifts)
            {
                if (!Validate(idAndGift.Value))
                {
                    errors.Add(idAndGift.Key);
                    allValid = false;
                }
            }

            return allValid;
        }

        public bool Validate(Gift gift)
        {
            if (gift == null)
            {
                return false;
            }

            if (gift.ItemName == null || gift.Amount <= 0 || gift.Traits == null)
            {
                return false;
            }

            return true;
        }
    }
}
