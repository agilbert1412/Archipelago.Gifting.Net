# Gifting API

## Overview

The Archipelago Gifting API is a system built using the DataStorage. It allows Archipelago players to send gifts to each other, across games.
A gift can be any item from any game, but client mods should take care to not allow gifting items that cannot be obtained again, to avoid a player getting softlocked.

Gifts will commonly be consumable or other quality of life items that a player doing well can send to help a player that is struggling.

## Cross Game Gifting

When a gift is sent between two players playing the same game, it can usually be taken as-is. But when a gift is sent cross-game, it is not always possible to establish a fair, 1-to-1 relationship between items from different games.

For this case, gifts also include a list of **traits**. A trait is a simple text-based flag for the gift describing one of its properties. If a sender game establishes a proper set of traits for a given gift, then a completely different game can parse these traits and interpret the gift as faithfully as possible in their own environment.

For example, a coffee item could have the trait "Speed". The concept of a speed boost is much more generic, and frequently encountered, than the concept of a coffee, so a larger set of games will be able to understand a coffee gift. A game that has espresso might convert it to that, while a game that has no coffee-adjacent items could simply give the player a temporary speed boost.

## 