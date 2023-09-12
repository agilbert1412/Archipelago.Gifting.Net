using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archipelago.Gifting.Net.DTO.Version2
{
    public class Validator
    {
        public Validator()
        {

        }

        public bool Validate(Dictionary<Guid, Gift> gifts, out IList<Guid> errors)
        {
            errors = new List<Guid>();
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
