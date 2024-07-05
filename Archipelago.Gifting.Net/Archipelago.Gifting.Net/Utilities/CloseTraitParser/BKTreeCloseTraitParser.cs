using System;
using System.Collections.Generic;
using Archipelago.Gifting.Net.Traits;

namespace Archipelago.Gifting.Net.Utilities.CloseTraitParser
{
    public class BKTreeCloseTraitParser<T> : ICloseTraitParser<T>
    {
        private readonly List<T> _items;
        private readonly Dictionary<string, Tuple<double, double>> _traits;
        private readonly Dictionary<double, BKTreeCloseTraitParser<T>> _children;

        public delegate double DistanceDelegate(GiftTrait[] giftTraits,
            Dictionary<string, Tuple<double, double>> traits,
            out bool isCompatible);

        private readonly DistanceDelegate _distance;

        public BKTreeCloseTraitParser(DistanceDelegate distanceDelegate = null)
        {
            _items = new List<T>();
            _traits = new Dictionary<string, Tuple<double, double>>();
            _children = new Dictionary<double, BKTreeCloseTraitParser<T>>();
            _distance = distanceDelegate ?? DefaultDistance;
        }

        private static double DefaultDistance(GiftTrait[] giftTraits, Dictionary<string, Tuple<double, double>> traits,
            out bool isCompatible)
        {
            Dictionary<string, Tuple<double, double>>
                traitsCopy = new Dictionary<string, Tuple<double, double>>(traits);
            double distance = 0;
            foreach (GiftTrait giftTrait in giftTraits)
            {
                if (traitsCopy.TryGetValue(giftTrait.Trait, out Tuple<double, double> values))
                {
                    traitsCopy.Remove(giftTrait.Trait);
                    if (values.Item1 * giftTrait.Quality <= 0)
                    {
                        distance += 1;
                    }
                    else
                    {
                        double d = values.Item1 / giftTrait.Quality;
                        distance += 1 - (d > 1 ? 1 / d : d);
                    }

                    if (values.Item2 * giftTrait.Duration <= 0)
                    {
                        distance += 1;
                    }
                    else
                    {
                        double d = values.Item2 / giftTrait.Duration;
                        distance += 1 - (d > 1 ? 1 / d : d);
                    }
                }
                else
                {
                    distance += 1;
                }
            }

            distance += traitsCopy.Count;
            isCompatible = traitsCopy.Count != traits.Count;
            return distance;
        }

        public void RegisterAvailableGift(T availableGift, GiftTrait[] traits)
        {
            if (_items.Count == 0)
            {
                _items.Add(availableGift);
                foreach (GiftTrait giftTrait in traits)
                {
                    if (_traits.TryGetValue(giftTrait.Trait, out Tuple<double, double> values))
                    {
                        _traits[giftTrait.Trait] = new Tuple<double, double>(values.Item1 + giftTrait.Quality,
                            values.Item2 + giftTrait.Duration);
                    }
                    else
                    {
                        _traits[giftTrait.Trait] = new Tuple<double, double>(giftTrait.Quality, giftTrait.Duration);
                    }
                }

                return;
            }

            double distance = _distance(traits, _traits, out _);
            if (distance == 0)
            {
                _items.Add(availableGift);
                return;
            }

            if (!_children.TryGetValue(distance, out BKTreeCloseTraitParser<T> child))
            {
                child = new BKTreeCloseTraitParser<T>(_distance);
                _children[distance] = child;
            }

            child.RegisterAvailableGift(availableGift, traits);
        }

        private void FindClosestAvailableGift(GiftTrait[] giftTraits, ref double bestDistance, ref List<T> closestItems)
        {
            double distance = _distance(giftTraits, _traits, out bool isCompatible);
            if (isCompatible)
            {
                if (Math.Abs(distance - bestDistance) < 0.0001)
                {
                    closestItems.AddRange(_items);
                }
                else if (distance < bestDistance)
                {
                    closestItems.Clear();
                    closestItems.AddRange(_items);
                    bestDistance = distance;
                }
            }

            foreach (KeyValuePair<double, BKTreeCloseTraitParser<T>> keyValuePair in _children)
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
