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
The Motherbox can be access in Data storage at the key "GiftBoxes;[teamNumber]"

Both the Motherbox and individual giftboxes are dictionaries. The motherbox contains player slot numbers as keys, and giftbox metadata as values. A giftbox has the gift's GUID as keys, and gifts as IDs.

## Object Specifications

### Giftbox Metadata Specification

| Field          | Type           | Description                                                                                                                                                    |
|----------------|----------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------|
| IsOpen         | Boolean        | If the giftbox is currently open. Gifts should not be sent to closed giftboxes                                                                                 |
| AcceptsAnyGift | Boolean        | Whether this player can and will try to process **any** gift sent to them. If false, only gifts from the same game or following the DesiredTraits are accepted |
| DesiredTraits  | List of String | The list of traits that this giftbox can process. If "AcceptsAnyGift" is true, these traits can remain empty, or be used to express preferences                |

### Gift Specification

| Field             | Type               | Description                                                                        |
|-------------------|--------------------|------------------------------------------------------------------------------------|
| ID                | GUID               | Unique ID for the Gift                                                             |
| Item              | GiftItem           | Item being gifted (see [Gift Item Specification](#giftitem-specification))         |
| Traits            | List of GiftTraits | Traits of the gift (see [Gift Trait Specification](#gifttrait-specification))      |
| SenderName        | String             | Slot Name of the player sending the gift                                           |
| ReceiverName      | String             | Slot Name of the player receiving the gift                                         |
| SenderTeam        | Integer            | Team Number of the player sending the gift                                         |
| ReceiverTeam      | Integer            | Team Number of the player receiving the gift                                       |
| IsRefund          | Boolean            | Flag describing if the gift is an original, or a refund for a previously sent gift |
| GiftValue         | Integer            | Total value of the gift (Item Value \* Item Amount)                                |

### GiftItem Specification

| Field             | Type           | Description                                                                        |
|-------------------|----------------|------------------------------------------------------------------------------------|
| Name              | String         | Name of the Item                                                                   |
| Amount            | Integer        | Amount of the Item being gifted                                                    |
| Value             | Integer        | Value per unit of the item                                                         |

### GiftTrait Specification

| Field             | Type           | Description                                                                        |
|-------------------|----------------|------------------------------------------------------------------------------------|
| Trait             | String         | Identifier for the Trait                                                           |
| Quality           | Float           | How powerful the Trait is (1.0 means "normal power")                                      |
| Duration          | Float          | Duration of the Trait (1.0 means "normal duration")                                |

## Examples

### Motherbox

```json
"GiftBoxes;0":
{
	"1":
	{
		"IsOpen": true,
		"AcceptsAnyGift": true,
		"DesiredTraits": ["Seed", "Speed", "Heal", "Metal", "Bomb"]
	},
	"2":
	{
		"IsOpen": false,
		"AcceptsAnyGift": false,
		"DesiredTraits": ["Food", "Consumable", "Bomb", "Weapon", "Tool", "Metal", "Fish"]
	},
	"3":
	{
		"IsOpen": true,
		"AcceptsAnyGift": false,
		"DesiredTraits": ["Speed", "Slow", "Buff", "Consumable"]
	}
}
"GiftBoxes;1":
{
	"1":
	{
		"IsOpen": true,
		"AcceptsAnyGift": true,
		"DesiredTraits": ["Seed", "Speed", "Heal", "Metal", "Bomb"]
	}
}
```

### Gifts

The Factorio player sent copper plates to the Stardew Valley player to help them with a tool upgrade.

The Factorio player sent iron plates to the Witness player, but the gift was refunded as The Witness had no good way to process Iron Plates.

The Stardew Valley player sent coffee to the Witness player to give them a speed boost.
```json
"GiftBox;0;1":
{
	"45703834-0906-45df-a1f2-88a728a79f17":
	{
		"ID": "45703834-0906-45df-a1f2-88a728a79f17",
		"Item":
		{
			"Name": "Copper Plate",
			"Amount": 5,
			"Value": 288000
		},
		"Traits":
		[
			{
				"Trait": "Metal",
				"Quality": 1,
				"Duration": 1
			},
			{
				"Trait": "Copper",
				"Quality": 1,
				"Duration": 1
			}
		],
		"Sender": "Engineer",
		"Receiver": "Farmer",
		"SenderTeam": 0,
		"ReceiverTeam": 0,
		"IsRefund": false,
		"GiftValue": 1440000
	},
},
"GiftBox;0;2":
{
	"99364460-e1d4-4777-a28d-5e86e62cae82":
	{
		"ID": "99364460-e1d4-4777-a28d-5e86e62cae82",
		"Item":
		{
			"Name": "Iron Plate",
			"Amount": 5,
			"Value": 288000
		},
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
		"Sender": "Engineer",
		"Receiver": "Carl",
		"SenderTeam": 0,
		"ReceiverTeam": 0,
		"IsRefund": true,
		"GiftValue": 1440000
	},
}
"GiftBox;0;3":
{
	"1991ec4b-2651-4260-83c6-beda93367d79":
	{
		"ID": "1991ec4b-2651-4260-83c6-beda93367d79",
		"Item":
		{
			"Name": "Coffee",
			"Amount": 1,
			"Value": 1500000000
		},
		"Traits":
		[
			{
				"Trait": "Drink",
				"Quality": 1,
				"Duration": 1
			},
			{
				"Trait": "Speed",
				"Quality": 1,
				"Duration": 2
			}
		],
		"Sender": "Farmer",
		"Receiver": "Carl",
		"SenderTeam": 0,
		"ReceiverTeam": 0,
		"IsRefund": false,
		"GiftValue": 1500000000
	}
}
```
And, Gifts can also be intended as traps for a player on another team
```json
"GiftBox;1;1":
{
	"06e8cc07-2989-4011-b4b6-794ceba25f28":
	{
		"ID": "06e8cc07-2989-4011-b4b6-794ceba25f28",
		"Item":
		{
			"Name": "Mega Bomb",
			"Amount": 1,
			"Value": 500000000
		},
		"Traits":
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
		"Sender": "Farmer",
		"Receiver": "EnemyFarmer",
		"SenderTeam": 0,
		"ReceiverTeam": 1,
		"IsRefund": false,
		"GiftValue": 500000000
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