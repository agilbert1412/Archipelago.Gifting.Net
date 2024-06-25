using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.Gifting.Net.Gifts;
using Archipelago.Gifting.Net.Traits;

namespace Archipelago.Gifting.Net.Utilities.CloseTraitParser
{
    public class BKTreeCloseTraitParser : ICloseTraitParser
    {
        private readonly List<GiftItem> Items;
        private readonly Dictionary<string, Tuple<double, double>> Traits;
        private readonly Dictionary<double, BKTreeCloseTraitParser> Children;

        public BKTreeCloseTraitParser()
        {
            Items = new List<GiftItem>();
            Traits = new Dictionary<string, Tuple<double, double>>();
            Children = new Dictionary<double, BKTreeCloseTraitParser>();
        }

        private double Distance(GiftTrait[] giftTraits)
        {
            Dictionary<string, Tuple<double, double>> recordedTraits =
                new Dictionary<string, Tuple<double, double>>(Traits);
            double distance = 0;
            foreach (GiftTrait giftTrait in giftTraits)
            {
                if (recordedTraits.TryGetValue(giftTrait.Trait, out Tuple<double, double> values))
                {
                    recordedTraits[giftTrait.Trait] = new Tuple<double, double>(values.Item1 - giftTrait.Quality,
                        values.Item2 - giftTrait.Duration);
                }
                else
                {
                    distance += giftTrait.Quality + giftTrait.Duration;
                }
            }

            distance += recordedTraits.Sum(keyValuePair =>
                Math.Abs(keyValuePair.Value.Item1) + Math.Abs(keyValuePair.Value.Item2));
            return distance;
        }

        public void RegisterGiftItem(GiftItem item, GiftTrait[] giftTraits)
        {
            if (Items.Count == 0)
            {
                Items.Add(item);
                foreach (GiftTrait giftTrait in giftTraits)
                {
                    if (Traits.TryGetValue(giftTrait.Trait, out Tuple<double, double> values))
                    {
                        Traits[giftTrait.Trait] = new Tuple<double, double>(values.Item1 + giftTrait.Quality,
                            values.Item2 + giftTrait.Duration);
                    }
                    else
                    {
                        Traits[giftTrait.Trait] = new Tuple<double, double>(giftTrait.Quality, giftTrait.Duration);
                    }
                }
                return;
            }

            double distance = Distance(giftTraits);
            if (distance == 0)
            {
                Items.Add(item);
                return;
            }

            if (!Children.TryGetValue(distance, out BKTreeCloseTraitParser child))
            {
                child = new BKTreeCloseTraitParser();
                Children[distance] = child;
            }

            child.RegisterGiftItem(item, giftTraits);
        }

        private void GetItems(GiftTrait[] giftTraits, ref double bestDistance, ref List<GiftItem> closestItems)
        {
            double distance = Distance(giftTraits);
            if (Math.Abs(distance - bestDistance) < 0.0001)
            {
                closestItems.AddRange(Items);
            }
            else if (distance < bestDistance)
            {
                closestItems.Clear();
                closestItems.AddRange(Items);
                bestDistance = distance;
            }
            
            foreach (KeyValuePair<double,BKTreeCloseTraitParser> keyValuePair in Children)
            {
                if (distance - keyValuePair.Key < bestDistance + 0.0001)
                {
                    keyValuePair.Value.GetItems(giftTraits, ref bestDistance, ref closestItems);
                }
            }
        }

        public List<GiftItem> FindClosest(GiftTrait[] giftTraits)
        {
            List<GiftItem> closestItems = new List<GiftItem>();
            double bestDistance = double.MaxValue;
            GetItems(giftTraits, ref bestDistance, ref closestItems);
            return closestItems;
        }
    }
}
