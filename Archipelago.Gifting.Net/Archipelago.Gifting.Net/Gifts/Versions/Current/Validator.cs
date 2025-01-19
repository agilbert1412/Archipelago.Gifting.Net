﻿using System.Collections.Generic;

namespace Archipelago.Gifting.Net.Gifts.Versions.Current
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
            foreach (var giftEntry in gifts)
            {
                if (!Validate(giftEntry.Value))
                {
                    errors.Add(giftEntry.Key);
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

            if (gift.itemName == null || gift.amount <= 0 || gift.traits == null)
            {
                return false;
            }

            return true;
        }
    }
}