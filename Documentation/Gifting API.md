# Gifting API

## Overview

The Archipelago Gifting API is a system built using the DataStorage. It allows Archipelago players to send gifts to each other, across games.
A gift can be any item from any game, but client mods should take care to not allow gifting items that cannot be obtained again, to avoid a player getting softlocked.

Gifts will commonly be consumable or other quality of life items that a player doing well can send to help a player that is struggling.

## Cross Game Gifting

When a gift is sent between two players playing the same game, it can usually be taken as-is. But when a gift is sent cross-game, it is not always possible to establish a fair, 1-to-1 relationship between items from different games.

For this case, gifts also include a list of **traits**. A trait is a simple text-based flag for the gift describing one of its properties. If a sender game establishes a proper set of traits for a given gift, then a completely different game can parse these traits and interpret the gift as faithfully as possible in their own environment.

For example, a coffee item could have the trait "Speed". The concept of a speed boost is much more generic, and frequently encountered, than the concept of a coffee, so a larger set of games will be able to understand a coffee gift. A game that has espresso might convert it to that, while a game that has no coffee-adjacent items could simply give the player a temporary speed boost.

## Giftbox

A GiftBox is a DataStorage entry registered to a specific player. It signals the desire and ability to receive gifts for that slot.

The key for a GiftBox is formatted as `GiftBox;[teamNumber];[slotName]`. So for player 3 on team 1, their GiftBox key would be "GiftBox;1;3". A giftbox also has metadata that is registered in the "Motherbox" for the team, describing the state of the giftbox, what kind of gifts it can accept, who owns it, etc.
The Motherbox can be accessed in Data storage at the key "GiftBoxes;[teamNumber]"

Both the Motherbox and individual giftboxes are dictionaries. The motherbox contains player slot numbers as keys, and giftbox metadata as values. A giftbox has the gift's Ids as keys, and gifts as values.

## Object Specifications

These specifications are **Data Version 3**. Previous versions are available in the git history of this file, and the changes introduced are documented in [the changelog document](Changelog.md), but clients should try to stay up to date, as cross-version gifting takes a lot of extra work that most clients will not do.
The C# library available in this repo is fully forward and backward compatible. It can send and receive outdated gifts to and from outdated clients, and is itself forward compatible and will understand content from future versions of itself.

### Giftbox Metadata Specification

| Field                     | Type           | Description                                                                                                                                                                                                             |
|---------------------------|----------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| is_open                   | Boolean        | If the giftbox is currently open. Gifts should not be sent to closed giftboxes                                                                                                                                          |
| accepts_any_gift          | Boolean        | Whether this player can and will try to process **any** gift sent to them. If false, only gifts from the same game or following the desired_traits are accepted                                                         |
| desired_traits            | List of String | The list of traits that this giftbox can process. If "accepts_any_gift" is true, these traits can remain empty, or be used to express preferences                                                                       |
| minimum_gift_data_version | Integer        | The minimum data version that this giftbox will accept. Gifts that have been created using an older data version than this value should not be sent to this giftbox.                                                    |
| maximum_gift_data_version | Integer        | The maximum data version that this giftbox will accept. Gifts that have been created using a newer data version than this value should not be sent to this giftbox. Some clients can generate older gifts to accomodate |

### Gift Specification

| Field              | Type               | Description                                                                                                                                                    |
|--------------------|--------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------|
| id                 | String             | Unique ID for the Gift. This should be a Globally unique Id. The recommended format is a GUID "00000000-0000-0000-0000-000000000000"                           |
| item_name          | String             | Name of the Item                                                                                                                                               |
| amount             | Integer            | Amount of the Item being gifted. Must be a positive integer.                                                                                                   |
| item_value         | Unbounded Integer  | Value per unit of the item. in Archipelago Currency (EnergyLink). Can be used to "sell" gifts that cannot be received properly. Can be omitted.\*              |
| traits             | List of GiftTraits | Traits of the gift (see [Gift Trait Specification](#gifttrait-specification)). Can be empty\*\*, but at least one trait is highly recommended                  |
| sender_slot        | Integer            | Slot Number of the player sending the gift                                                                                                                     |
| receiver_slot      | Integer            | Slot Number of the player receiving the gift                                                                                                                   |
| sender_team        | Integer            | Team Number of the player sending the gift                                                                                                                     |
| receiver_team      | Integer            | Team Number of the player receiving the gift                                                                                                                   |
| is_refund          | Boolean            | Flag describing if the gift is an original, or a refund for a previously sent gift                                                                             |

\* An item with no value should not be sold. If absolutely necessary, no value can be interpreted as equivalent to zero.

\*\* If a gift has no traits, it can only be parsed by name. In some cases, notably same-game gifting, this is acceptable. But across games, traits are much preferable because of their versatility. For example, two items can have the same name but be significantly different. Or two items can be extremely similar, but not share a name (HP Potion / Health Potion / Heal Potion)

### GiftTrait Specification

| Field             | Type             | Description                                                                                              |
|-------------------|------------------|----------------------------------------------------------------------------------------------------------|
| trait             | String           | Identifier for the Trait                                                                                 |
| quality           | Float\*          | How powerful the Trait is (1.0 means "average power"). Can be omitted.\*\*                               |
| duration          | Float\*          | Duration of the Trait (1.0 means "average duration"). Can be omitted.\*\*                                |

\* These values should be floating point numbers that are scaled around "1.0", as a factor of it.

\*\* If omitted, these fields should be considered to be their default value of 1.0

1.0 should represent the average item with this trait in your world. Scaling linearly is recommended but not required, for example an item with 2.0 is twice as strong, and one with 0.33 is 3 times weaker. Games with exponential scaling, for example idle games, might decide on a different scaling system for their traits in order to not cause massive inflation.

While the format technically allows for zero or negative values, it is **recommended** to stick to strictly positive values, as there is no official definition for what 0 or -1 would mean. Games can use it to handle special cases if they have any, but should expect other games to not necessarily understand it.

## Examples

### Motherbox

```json
"GiftBoxes;0":
{
	"1":
	{
		"is_open": true,
		"accepts_any_gift": true,
		"desired_traits": ["Seed", "Speed", "Heal", "Metal", "Bomb"]
		"minimum_gift_data_version": 1
		"maximum_gift_data_version": 3
	},
	"2":
	{
		"is_open": false,
		"accepts_any_gift": false,
		"desired_traits": ["Food", "Consumable", "Bomb", "Weapon", "Tool", "Metal", "Fish"]
		"minimum_gift_data_version": 2
		"maximum_gift_data_version": 3
	},
	"3":
	{
		"is_open": true,
		"accepts_any_gift": false,
		"desired_traits": ["Speed", "Slow", "Buff", "Consumable"]
		"minimum_gift_data_version": 1
		"maximum_gift_data_version": 1
	}
}
"GiftBoxes;1":
{
	"1":
	{
		"is_open": true,
		"accepts_any_gift": true,
		"desired_traits": ["Seed", "Speed", "Heal", "Metal", "Bomb"]
		"minimum_gift_data_version": 1
		"maximum_gift_data_version": 3
	}
}
```

### Gifts

The DLC Quest player sent a tree to the Stardew Valley player to help them with crafting

The Satisfactory player sent iron plates to Kirby's Dream Land, but the gift was refunded as Kirby's Dream Land had no good way to process Iron Plates.

The Stardew Valley player sent a tomato to Kirby's Dream Land, and it will be interpreted as a Maxim Tomato.

```json
"GiftBox;0;2":
{
	"45703834-0906-45df-a1f2-88a728a79f17":
	{
		"id": "45703834-0906-45df-a1f2-88a728a79f17",
		"item_name": "Tree",
		"amount": 1,
		"item_value": 0,
		"traits":
		[
			{
				"trait": "Wood",
				"quality": 1,
				"duration": 1
			},
			{
				"trait": "Material",
				"quality": 1,
				"duration": 1
			}
		],
		"sender_slot": 1,
		"receiver_slot": 2,
		"sender_team": 0,
		"receiver_team": 0,
		"is_refund": false,
	},
},
"GiftBox;0;3":
{
	"99364460-e1d4-4777-a28d-5e86e62cae82":
	{
		"id": "99364460-e1d4-4777-a28d-5e86e62cae82",
		"item_name": "Iron Plate",
		"Amount": 5,
		"ItemValue": 288000,
		"Traits":
		[
			{
				"Trait": "Metal",
				"Quality": 1,
				"Duration": 1
			},
			{
				"Trait": "Iron",
				"Quality": 1,
				"Duration": 1
			}
		],
		"sender_slot": 3,
		"receiver_slot": 4,
		"sender_team": 0,
		"receiver_team": 0,
		"is_refund": true,
	},
}
"GiftBox;0;4":
{
	"1991ec4b-2651-4260-83c6-beda93367d79":
	{
		"id": "1991ec4b-2651-4260-83c6-beda93367d79",
		"item_name": "Tomato",
		"amount": 1,
		"item_value": 600000000,
		"traits":
		[
			{
				"Trait": "Vegetable",
				"Quality": 1,
				"Duration": 1
			},
			{
				"Trait": "Consumable",
				"Quality": 1,
				"Duration": 1
			}
			{
				"Trait": "Food",
				"Quality": 1,
				"Duration": 1
			}
			{
				"Trait": "Red",
				"Quality": 1,
				"Duration": 1
			}
			{
				"Trait": "Dye",
				"Quality": 1,
				"Duration": 1
			}
			{
				"Trait": "Summer",
				"Quality": 1,
				"Duration": 1
			}
		],
		"sender_slot": 2,
		"receiver_slot": 4,
		"sender_team": 0,
		"receiver_team": 0,
		"is_refund": false,
	}
}
```
And, Gifts can also be intended as traps for a player on another team
```json
"GiftBox;1;1":
{
	"06e8cc07-2989-4011-b4b6-794ceba25f28":
	{
		"id": "06e8cc07-2989-4011-b4b6-794ceba25f28",
		"item_name": "Mega Bomb",
		"amount": 1,
		"item_value": 500000000,
		"traits":
		[
			{
				"Trait": "Bomb",
				"Quality": 3,
				"Duration": 1
			},
			{
				"Trait": "Damage",
				"Quality": 1.5,
				"Duration": 1
			},
			{
				"Trait": "Trap",
				"Quality": 3,
				"Duration": 1
			}
		],
		"sender_slot": 1,
		"receiver_slot": 1,
		"sender_team": 0,
		"receiver_team": 1,
		"is_refund": false,
	}
}
```

## Gift Traits

A gift trait can be any string, and as long as it is recognized by the target game, it can be processed however they would like.

For the sake of consistency and ease of use however, it is recommend to use single words that describe a concept in a very generic, vague manner.
The preferred method of describing a gift in details is to add many vague traits, instead of few specific traits. This will allow other games to also limit the number of different traits they need to understand in order to parse gifts.

Here is a list of "common" Gift traits. Everything on this list is a suggestion, and there are no strict rules as to how to attribute or process gift traits. Common sense and good faith are expected.

| Trait             | Description                                                                          |
|-------------------|--------------------------------------------------------------------------------------|
| Speed             | Increases Speed                                                                      |
| Consumable        | Can be consumed                                                                      |
| Food              | Can eat                                                                              |
| Drink             | Can drink                                                                            |
| Heal              | Heals                                                                                |
| Mana              | Restores mana                                                                        |
| Key               | Can open a door, a chest, can unlock something                                       |
| Trap              | The gift is intended as negative. Other traits should be interpreted as such.        |
| Buff              | Buffs the player or some aspect of them                                              |
| Life              | Grants an extra life or increases max HP                                             |
| Weapon            | Is a weapon, can be used for combat                                                  |
| Armor             | Is an armor or other wearable item, increases defense                                |
| Tool              | Is a tool, can be used to complete tasks                                             |
| Fish              | Is a fish, or related to fishing                                                     |
| Animal            | Is an animal, pet, companion                                                         |
| Cure              | Cures status effect, illnesses, ailments                                             |
| Seed              | Can be planted, farming-related                                                      |
| Metal             | Material for crafting, iron, copper, steel, etc                                      |
| Bomb              | Can explode, be used as a weapon, to destroy things, etc                             |
| Monster           | Enemy NPC, mob, something to be killed                                               |
| Resource          | An item that can be used to build or purchase something, or complete a task          |
| Material          | An item to be used for crafting, construction                                        |
| Wood              | Wood, Lumber, tree-based products                                                    |
| Stone             | Stone, Rock, Boulder, rock-based products                                            |
| Ore               | Precious, raw material for a metal or other processed material                       |
| Grass             | Grass, Leaves, related to nature                                                     |
| Meat              | Subcategory for food related to meats, animal products                               |
| Vegetable         | Subcategory for food that are vegetables, salads, etc                                |
| Fruit             | Subcategory for food that are fruits, foraged, etc                                   |
| Egg               | Animal Product, container that can spawn something                                   |
| Slowness          | Status effect, opposite of speed                                                     |
| Damage            | Deals damage, reduce HP                                                              |
| Fire              | Status effect, warm, heat, burn                                                            |
| Ice               | Status effect, cold, freeze, slow                                                             |
| Currency          | Item that can be used as currency. Consider using the Energylink instead of gifts    |
| Energy            | Stores or is Energy, electricity. Consider using the EnergyLink instead of gifts     |
| Light             | Is a light source or light itself, can help against darkness                         |
