# Archipelago.Gifting.Net
Gifting Library for use in .NET applications interfacing with the Archipelago Randomizer
This library uses [Archipelago.MultiClient.Net](https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net/tree/main) for network communication

This library provides a simple and easy way to interact with the [Gifting API](Documentation/Gifting%20API.md)

# Documentation

## Creating a GiftingService Instance

```cs
// session must be a previously-established, connected session from Archipelago.MultiClient.Net
var service = new GiftingService(session);
```

A freshly created GiftingService instance, on its own, does not do anything. It allows usage of various utility methods to interact with GiftBoxes and Gifts.

## Opening and Closing a GiftBox

To inform the multiworld that your slot is willing and able to receive gifts, you must open a GiftBox.
Using the default method will open a gift box that is marked as able to accept any gift with no preferences.
You can also use the alternate method to specify whether you can handle any gift. If not, you must specify which traits you can accept. If so, you can still specify preferences for traits.
This will open a giftBox on the Data Version used by your currently installed version of the library. It is recommended to stay up to date, but the C# library will make a reasonable attempt at parsing and generating gifts for older versions when it detects that they exist, for backward compatibility.

Your giftBox will always be considered able to receive gifts from the same game. You can also process gifts by name if you wish.

```cs
_service.OpenGiftBox();
_service.OpenGiftBox(false, new [] {"Food", "Drink", "Speed"})
_service.CloseGiftBox();
```

Once a giftBox is open, you can close it at any time, but you do not have to. You can leave your giftBox open for as long as you wish, even across different sessions.
An open giftBox can receive gifts at any time. If your game can only receive gifts while online (sync), you should close the giftBox when disconnecting to prevent receiving gifts while offline. If your game can receive gifts while offline, you can keep the giftBox open forever and simply check on the gifts when logging in.
Closing a giftBox will delete all of its content, so make sure you empty it first to avoid losing gifts.

## Receiving Gifts

```cs
// Get all gifts
var gifts = _service.CheckGiftBox();

// Get all gifts and empty the giftBox immediately
var gifts = _service.GetAllGiftsAndEmptyGiftBox();

// Remove one specific gift from the giftBox
_service.RemoveGiftFromGiftBox(giftId);

// Remove multiple gifts at once
_service.RemoveGiftsFromGiftBox(giftIds);
```

At any point, you can query your own giftBox for gifts. If the giftBox is currently closed, the result will always be empty.
The result will be a dictionary of IDs and gifts (`Dicionary<string, Gift>`), with the entire content of the giftBox.

You have the responsibility to clean your giftBox yourself once your client has processed the gifts.
You can either use `GetAllGiftsAndEmptyGiftBox` to get gifts and immediately empty the giftBox, or if you need to do validation before deleting the content, you can use `CheckGiftBox` and `RemoveGiftsFromGiftBox` separately so you can do what you need between the calls.
You can also remove gifts one by one using `RemoveGiftFromGiftBox` if you prefer.

It is also possible to never empty your giftBox, and keep a local list of processed gift IDs to distinguish between new gifts and old ones. This method is not recommended, because it will leave pointless data in the server storage potentially for a very long time.

It is, however, recommended to keep the list of processed gift IDs anyway, in case of a race condition with the clearing of the giftBox.

The `GiftingService` also offers async variants of most methods.

## Set up notifications

After processing the content of your giftBox upon logging on, you can use the event `OnNewGift` to get notified for future gifts, instead of checking it again yourself.
This method is not mandatory, checking at regular intervals is also valid. Which strategy is better for you depends on your implementation and your needs.

Here is an example of how to subscribe to get notified. You will need to provide the implementation of the Callback method
```cs
void NewGiftsCallback(Gift gift)
{
	// Process the gift
}

_service.OnNewGift += NewGiftsCallback;
```

Like all events, you can also unsubscribe to it if you need to. You can subscribe multiple handlers if you need.
```cs
_service.OnNewGift -= NewGiftsCallback;
```

Definition of a gift in the current Data Version 3:
```cs
public class Gift
{
	[JsonProperty(propertyName: "id")]
	public string ID { get; set; }

	[JsonProperty(propertyName: "item_name")]
	public string ItemName { get; set; }

	[JsonProperty(propertyName: "amount")]
	public int Amount { get; set; }
	
	[JsonProperty(propertyName: "item_value")]
	public BigInteger ItemValue { get; set; }

	[JsonProperty(propertyName: "traits")]
	public GiftTrait[] Traits { get; set; }

	[JsonProperty(propertyName: "sender_slot")]
	public int SenderSlot { get; set; }

	[JsonProperty(propertyName: "receiver_slot")]
	public int ReceiverSlot { get; set; }

	[JsonProperty(propertyName: "sender_team")]
	public int SenderTeam { get; set; }

	[JsonProperty(propertyName: "receiver_team")]
	public int ReceiverTeam { get; set; }

	[JsonProperty(propertyName: "is_refund")]
	public bool IsRefund { get; set; }
}
```

## Creating Gifts

To send a gift, you first need to create a GiftItem, and optionally, Gift Traits. If you do not define GiftTraits, the gift will be sent without any traits, and can only be parsed by name.

```cs
public class GiftItem
{
	public string Name { get; }
	public int Amount { get; }
	public BigInteger Value { get; }

	public GiftItem(string name, int amount, BigInteger value)
	{
		Name = name;
		Amount = amount;
		Value = value;
	}
}
```

A gift item has a name, an amount and a value. It is important to note that the value is for one instance of the item. The total value of the gift will be the value multiplied by the amount.

A gift can have as many traits as you wish, and it is up to the receiver to decide how to interpret these traits.

It is recommended to add more traits rather than fewer, so it is more likely to be understandable by various games.

It is not recommended to add many synonymous, or very similar, traits, as this makes it more complicated for the multiworld developers to keep track of commonly used traits that they should support. For example, a gift should probably not carry both the trait "Stone" and the trait "Rock". "Stone", being a "common" trait based on the specification, is preferable.

```cs
public class GiftTrait
{
	[JsonProperty(propertyName: "trait")]
	public string Trait { get; set; }

	[JsonProperty(propertyName: "quality")]
	public double Quality { get; set; }

	[JsonProperty(propertyName: "duration")]
	public double Duration { get; set; }

	public GiftTrait(string trait) : this(trait, 1.0)
	{
	}

	public GiftTrait(string trait, double quality) : this(trait, quality, 1.0)
	{
	}

	public GiftTrait(string trait, double quality, double duration)
	{
		Trait = trait;
		Quality = quality;
		Duration = duration;
	}
}
```

A Trait is defined by a string describing the trait itself. It is usually a single word. While you can put anything there, some common traits are available in the class `GiftFlag` as constants, for convenience.

Furthermore, a trait has two extra values, which are the quality and the duration of the trait. What these values mean exactly will depend on the game, but it is intended that a value of `1.0` describes an average quality or duration for a given game.

For example, if your game contains speed boosts that can last 30s, 60s or 90s, then a "Speed" trait of duration 1.0 would be a 60s Speed boost. A duration of 0.5 would be 30s, 1.5 would be 90s, and if your mod can generate these with custom values, you could interpret a duration of 10.0 as 600s.

Once again, it is completely up to the various game developers to define what these values mean for their game. They are intended to convey a vague concept, not strict descriptions.

They should be used to distinguish characteristics of otherwise-similar items from the same game, and their values should always be considered relative, not absolute.

Both the quality and the duration are optional, and will default to 1.0 if omitted.

## Sending Gifts

Before attempting to send a gift, you can check if your desired receiver has an open GiftBox. You can provide the traits of your intended gift to also know if they can accept your gift specifically

```cs
// All parameters except the player are optional, but without providing traits, you will only know if the player can accept any gift from you, not a specific gift
public CanGiftResult CanGiftToPlayer(string playerName, int playerTeam, IEnumerable<string> giftTraits); // Definition.

var canGiftResult = _service.CanGiftToPlayer("playerSlotName"); // Usage
bool canGift = canGiftResult.CanGift;
bool canGiftMessage = canGiftResult.Message; // Error message only if CanGift is false
```

Checking the state of the giftBox before proceeding is optional, but recommended, to avoid pointless operations.

There are multiple methods to send a gift, but they are all variations of the following, with omitted optional parameters:

```cs
public GiftingResult SendGift(GiftItem item, GiftTrait[] traits, string playerName, int playerTeam); // Definition
var result = _servicer.SendGift(gift, giftTraits, targetSlotName); // Usage
var success = result.Success;
var id = result.GiftId;
```

Sending the gift generates a unique ID for it. The response will tell you if the operation was successful, and if so, what the ID of the gift is, for if you intend to tracking them.

The item and traits are the structure you have created in the previous steps. If traits are omitted, an empty array of traits will be set instead.
The player name is the SlotName of the player to send the gift to. This can also be their current alias. Alternatively, you can provide the player's slot number as an integer
The player's team is the team number. When omitted, it will default to your own team, which is generally the desired behavior.

The returned value is a boolean that depends on the success of the operation. True means that the gift was sent, false means that an error occured.
If the gift is not sent successfully, it is recommended not to take away the item from the local player.

## Processing a Gift

Games can choose to process gifts as they wish. They can use the item name, they can use the traits, or any combination of both. Usually, gifts that come from the same game can be parsed by name for maximum accuracy, but within reason, games should try to understand items from other games too.

Parsing by traits can be done however you wish. But the library offers a generic handler, called a "CloseTraitParser", that will allow you a simple, decent parsing system for traits into items from your game.

To use it, you must first initialize it, and then register every one of your possible receivable gifts in the parser, with their traits.
This list can come directly from the gifts you can send other players, or all in-game items, or any set you wish. But every item should have traits. The more items you have, the more traits you will need on the average item to get accurate parsing.

For example, if you have 10 items that all have the same traits, the Parser will not be able to distinguish them. The "Perfect" item list has every item with distinct traits. But even an imperfect list will work.

Your items can be any type you want, and the class is built as a generic so that you can tell it what types are your items. In the following example, the type `string` is used and the game is presumed to freely create items by name.

BKTreeCloseTraitParser is currently the only implementation of ICloseTraitParser, other implementations can be created by consumers of the API.

```cs
ICloseTraitParser<string> closeTraitParser = new BKTreeCloseTraitParser<string>();
// For all pairs of item and traits do
closeTraitParser.RegisterAvailableGift(item, traits);
```

The parser should be kept in memory for the whole duration of the session, to avoid having to register the same things over and over.
When you receive a gift that you wish to parse by traits, you can do:
```cs
List<string> matches = closeTraitParser.FindClosestAvailableGift(gift.Traits);
```
This list might be empty if no item was found that shared any trait.

If you aren't pleased by the closeness algorithm, you may provide your own as an argument to BKTreeCloseTraitParser, having the following signature
```cs
double Distance(GiftTrait[] giftTraits, Dictionary<string, Tuple<double, double>> traits, out bool isCompatible);
```
For this method, all the traits of the registered gift with the same name have been added together for performance reasons

## Rejecting a Gift

If you receive a gift that you cannot or will not process properly in the current game, 3 options are available to you.

1: Refunding that gift.

```cs
public bool RefundGift(Gift gift); // Definition
var result = _service.RefundGift(unwantedGift); // Usage
```

When refunding a gift, that gift will be sent back to the original sender, with the flag `IsRefund` now set to `true`, so they know it is not a new gift, but a refund of a gift they originally sent.
It is then up to the original sender client to decide what to do with it. Typically, they would simply give back the item to the player.

2: Selling the gift
The gift can carry a value in Archipelago currency, which can be added to the EnergyLink for everyone to use, if your game supports interacting with the EnergyLink. A value of zero should be interpreted as "coming from a game without multiworld currency", and selling is not a good choice for these gifts

3: Ignoring that gift completely. This should be a last resort, as the item will then be lost forever.
