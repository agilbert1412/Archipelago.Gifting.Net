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

The key for a GiftBox is formatted as `GiftBox;[slotName]`. So for a player named "Alice", her GiftBox key would be "GiftBox;Alice".
The value for a GiftBox Entry can be non-existent, if the GiftBox is closed or has never been opened. This signals that the player either does not exist or cannot receive gifts.
In programming, this will usually be a value along the lines of `Null`, `None`, `Nothing`, etc. depending on the language.
Or, it can exist, and then it should be a list of gifts. An empty list, or a populated list, both mean that the GiftBox is open and can receive gifts.

```json
"GiftBox;PlayerReadyToReceiveGiftsName": []
"GiftBox;PlayerWithGiftingTurnedOffName": null
```

## Object Specifications

### Gift Specification

| Field             | Type               | Description                                                                        |
|-------------------|--------------------|------------------------------------------------------------------------------------|
| ID                | GUID               | Unique ID for the Gift                                                             |
| Item              | GiftItem           | Item being gifted (see [Gift Item Specification](#giftitem-specification)).        |
| Traits            | List of GiftTraits | Traits of the gift (see [Gift Trait Specification](#gifttrait-specification))      |
| Sender            | String             | Slot Name of the player sending the gift                                           |
| Receiver          | String             | Slot Name of the player receiving the gift                                         |
| IsRefund          | Boolean            | Flag describing if the gift is an original, or a refund for a previously sent gift |
| GiftValue         | Integer            | Total value of the gift (Item Vale \* Item Amount)                                 |

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
| Strength          | Float          | Power of the Trait (1.0 means "normal power")                                      |
| Duration          | Float          | Duration of the Trait (1.0 means "normal duration")                                |

## Gifts Examples

```json
{
	"ID": "65ffdda5-b955-4711-9e62-a9627d2f24e1",
	"Item": {
		"Name": "Coffee",
		"Amount": 4,
		"Value": 9000
	},
	"Traits": [{
			"Trait": "Drink",
			"Strength": 1,
			"Duration": 1
		}, {
			"Trait": "Speed",
			"Strength": 1,
			"Duration": 2
		}
	],
	"Sender": "SenderName",
	"Receiver": "ReceiverName",
	"IsRefund": false,
	"GiftValue": 36000
}
```
```json
{
	"ID": "45703834-0906-45df-a1f2-88a728a79f17",
	"Item": {
		"Name": "Burn",
		"Amount": 1,
		"Value": 40
	},
	"Traits": [{
			"Trait": "Trap",
			"Strength": 1,
			"Duration": 1
		}, {
			"Trait": "Damage",
			"Strength": 5,
			"Duration": 2
		}, {
			"Trait": "Fire",
			"Strength": 5,
			"Duration": 1
		}
	],
	"Sender": "SenderName",
	"Receiver": "ReceiverName",
	"IsRefund": false,
	"GiftValue": 40
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
| Trap              | The gift is intended as negative. Other traits should be interpreted as such.        |
| Buff              | Buffs the player or some aspect of them                                              |
| Life              | Grants an extra life or increases max HP                                             |
| Weapon            | Is a weapon, can be used for combat                                                  |
| Armor             | Is an armor or other wearable item, increases defense                                |
| Tool              | Is a tool, can be used to complete tasks                                             |
| Animal            | Is an animal, pet, companion                                                         |
| Fish              | Is a fish, or related to fishing                                                     |