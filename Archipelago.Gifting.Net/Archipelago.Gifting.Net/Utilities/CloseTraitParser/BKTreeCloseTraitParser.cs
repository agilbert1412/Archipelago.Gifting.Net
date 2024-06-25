using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.Gifting.Net.Traits;

namespace Archipelago.Gifting.Net.Utilities.CloseTraitParser
{
    public class BKTreeCloseTraitParser<T> : ICloseTraitParser<T>
    {
        private readonly List<T> Items;
        private readonly Dictionary<string, Tuple<double, double>> Traits;
        private readonly Dictionary<double, BKTreeCloseTraitParser<T>> Children;

        public BKTreeCloseTraitParser()
        {
            Items = new List<T>();
            Traits = new Dictionary<string, Tuple<double, double>>();
            Children = new Dictionary<double, BKTreeCloseTraitParser<T>>();
        }

        private static double Distance(GiftTrait[] giftTraits, Dictionary<string, Tuple<double, double>> recordedTraits)
        {
            double distance = 0;
            int nRecordedTraits = recordedTraits.Count;
            foreach (GiftTrait giftTrait in giftTraits)
            {
                if (recordedTraits.TryGetValue(giftTrait.Trait, out Tuple<double, double> values))
                {
                    recordedTraits.Remove(giftTrait.Trait);
                    if (values.Item1 * giftTrait.Quality <= 0)
                    {
                        distance += 1;
                    }
                    else
                    {
                        distance += Math.Abs(Math.Log(values.Item1 / giftTrait.Quality));
                    }
                    if (values.Item1 * giftTrait.Duration <= 0)
                    {
                        distance += 1;
                    }
                    else
                    {
                        distance += Math.Abs(Math.Log(values.Item1 / giftTrait.Duration));
                    }
                }
                else
                {
                    distance += 1;
                }
            }

            if (nRecordedTraits == recordedTraits.Count)
                return double.PositiveInfinity;
            
            return distance + recordedTraits.Count;
        }

        private double Distance(GiftTrait[] giftTraits)
        {
            return Distance(giftTraits, new Dictionary<string, Tuple<double, double>>(Traits));
        }

        public void RegisterAvailableGift(T availableGift, GiftTrait[] traits)
        {
            if (Items.Count == 0)
            {
                Items.Add(availableGift);
                foreach (GiftTrait giftTrait in traits)
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

            double distance = Distance(traits);
            if (distance == 0)
            {
                Items.Add(availableGift);
                return;
            }

            if (!Children.TryGetValue(distance, out BKTreeCloseTraitParser<T> child))
            {
                child = new BKTreeCloseTraitParser<T>();
                Children[distance] = child;
            }

            child.RegisterAvailableGift(availableGift, traits);
        }

        private void FindClosestAvailableGift(GiftTrait[] giftTraits, ref double bestDistance, ref List<T> closestItems)
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

            foreach (KeyValuePair<double, BKTreeCloseTraitParser<T>> keyValuePair in Children)
            {
                if (distance - keyValuePair.Key < bestDistance + 0.0001)
                {
                    keyValuePair.Value.FindClosestAvailableGift(giftTraits, ref bestDistance, ref closestItems);
                }
            }
        }

        public List<T> FindClosestAvailableGift(GiftTrait[] giftTraits)
        {
            List<T> closestItems = new List<T>();
            double bestDistance = double.MaxValue;
            FindClosestAvailableGift(giftTraits, ref bestDistance, ref closestItems);
            return closestItems;
        }
    }
}
