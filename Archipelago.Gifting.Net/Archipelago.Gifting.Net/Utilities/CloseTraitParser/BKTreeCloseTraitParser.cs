using System;
using System.Collections.Generic;
using Archipelago.Gifting.Net.Versioning.Gifts.Current;

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

        public void CheckConsistency()
        {
            List<Tuple<BKTreeCloseTraitParser<T>, GiftTrait[]>> allBKTrees =
                new List<Tuple<BKTreeCloseTraitParser<T>, GiftTrait[]>>();
            List<BKTreeCloseTraitParser<T>> toTreatBKTrees = new List<BKTreeCloseTraitParser<T>> { this };
            for (int i = 0; i < toTreatBKTrees.Count; i++)
            {
                List<GiftTrait> traits = new List<GiftTrait>();
                foreach (KeyValuePair<string, Tuple<double, double>> keyValuePair in toTreatBKTrees[i]._traits)
                {
                    traits.Add(new GiftTrait(keyValuePair.Key, keyValuePair.Value.Item1, keyValuePair.Value.Item2));
                }

                allBKTrees.Add(new Tuple<BKTreeCloseTraitParser<T>, GiftTrait[]>(toTreatBKTrees[i], traits.ToArray()));
                toTreatBKTrees.AddRange(toTreatBKTrees[i]._children.Values);
            }

            foreach (Tuple<BKTreeCloseTraitParser<T>, GiftTrait[]> BKTree1 in allBKTrees)
            {
                GiftTrait[] traits1 = BKTree1.Item2;
                foreach (Tuple<BKTreeCloseTraitParser<T>, GiftTrait[]> BKTree2 in allBKTrees)
                {
                    BKTreeCloseTraitParser<T> tree2 = BKTree2.Item1;
                    GiftTrait[] traits2 = BKTree2.Item2;
                    
                    foreach (Tuple<BKTreeCloseTraitParser<T>, GiftTrait[]> BKTree3 in allBKTrees)
                    {
                        BKTreeCloseTraitParser<T> tree3 = BKTree3.Item1;
                        double d1 = _distance(traits1, tree2._traits, out bool _);
                        double d2 = _distance(traits2, tree3._traits, out bool _);
                        double d3 = _distance(traits1, tree3._traits, out bool _);

                        if (d1 + d2 - d3 < -0.00001) 
                            // Triangular inequality was violated, margin is smaller than in FindClosestAvailableGift
                        {
                            GiftTrait[] traits3 = BKTree3.Item2;
                            Exception exception = new Exception("Triangular inequalities were violated, " +
                                                                "d(traits1, traits2) + d(traits2, traits3) > d(traits1, traits3).\n" +
                                                                "Check this exception's data for more details.");
                            exception.Data.Add("traits1", traits1);
                            exception.Data.Add("traits2", traits2);
                            exception.Data.Add("traits3", traits3);
                            exception.Data.Add("d(traits1, traits2)", d1);
                            exception.Data.Add("d(traits2, traits3)", d2);
                            exception.Data.Add("d(traits1, traits3)", d3);
                            throw exception;
                        }
                    }
                }
            }
        }
    }
}