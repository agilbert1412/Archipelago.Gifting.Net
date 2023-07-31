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

A freshly created GiftingService instance, on its own, does not do anything. It allows usage of various utility methods to interact with Giftboxes and Gifts.

## Opening and Closing a Giftbox

To inform the multiworld that your slot is willing and able to receive gifts, you must open a Giftbox.
Using the default method will open a gift box that is marked as able to accept any gift with no preferences.
You can also use the alternate method to specify whether you can handle any gift. If not, you must specify which traits you can accept. If so, you can still specify preferences for traits.

Your giftbox will always be considered able to receive gifts from the same game. You can also process gifts by name if you wish.

```cs
_service.OpenGiftBox();
_service.OpenGiftBox(false, new [] {"Food", "Drink", "Speed"})
_service.CloseGiftBox();
```

Once a giftbox is open, you can close it at any time, but you do not have to. You can leave your giftbox open for as long as you wish, even across different sessions.
An open giftbox can receive gifts at any time. If your game can only receive gifts while online (sync), you should close the giftbox when disconnecting to prevent receiving gifts while offline. If your game can receive gifts while offline, you can keep the giftbox open forever and simply check on the gifts when logging in.
Closing a giftbox will delete all of its content, so make sure you empty it first to avoid losing gifts.

## Receiving Gifts

```cs
// Get all gifts
var gifts = _service.CheckGiftBox();

// Get all gifts and empty the giftbox immediately
var gifts = _service.GetAllGiftsAndEmptyGiftbox();

// Empty the giftbox, regardless of its content
_service.EmptyGiftBox();

// Remove one specific gift from the giftbox
_service.RemoveGiftFromGiftBox(giftGuid);
```

At any point, you can query your own giftbox for gifts. If the giftbox is currently closed, the result will always be empty.
The result will be a dictionary of GUIDs and gifts (`Dicionary<Guid, Gift>`), with the entire content of the giftbox.

You have the responsibility to clean your giftbox yourself once your client has processed the gifts.
You can either use `GetAllGiftsAndEmptyGiftbox` to get gifts and immediately empty the giftbox, or if you need to do validation before deleting the content, you can use `CheckGiftBox` and `EmptyGiftBox` separately so you can do what you need between the calls.
You can also remove gifts one by one using `RemoveGiftFromGiftBox` if you prefer.

It is also possible to never empty your giftbox, and keep a local list of processed gift IDs to distinguish between new gifts and old ones. This method is not recommended.

Definition of a gift:
```cs
public class Gift
{
	public Guid ID { get; }
	public GiftItem Item { get; }
	public GiftTrait[] Traits { get; }
	public string Sender { get; }
	public string Receiver { get; }
	public int SenderTeam { get; }
	public int ReceiverTeam { get; }
	public bool IsRefund { get; }
	public BigInteger GiftValue { get; }
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
A gift can have as many traits as you wish, and it is up to the receiver to decide how to interpret these traits. It is recommended to add more traits rather than fewer, so it is more likely to be understandable by various games.

```cs
public class GiftTrait
{
	public string Trait { get; set; }
	public double Quality { get; set; }
	public double Duration { get; set; }

	public GiftTrait(string trait, double duration, double quality)
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

## Sending Gifts

Before attempting to send a gift, you can check if your desired receiver has an open Giftbox. You can provide the traits of your intended gift to also know if they can accept your gift specifically

```cs
// All parameters except the player are optional, but without providing traits, you will only know if the player can accept any gift from you, not a specific gift
public bool CanGiftToPlayer(string playerName, int playerTeam, IEnumerable<string> giftTraits); // Definition.

var canGift = _service.CanGiftToPlayer("playerSlotName"); // Usage
```

Checking the state of the giftbox before proceeding is optional, but recommended, to avoid pointless operations.

There are multiple methods to send a gift, but they are all variations of the following, with omitted optional parameters:

```cs
public bool SendGift(GiftItem item, GiftTrait[] traits, string playerName, int playerTeam, out Guid giftId); // Definition
var result = _servicer.SendGift(gift, giftTraits, targetSlotName, out var giftId); // Usage
```

The item and traits are the structure you have created in the previous steps. If traits are omitted, an empty array of traits will be set instead.
The player name is the SlotName of the player to send the gift to. This can also be their current alias. Alternatively, you can provide the player's slot number as an integer
The player's team is the team number. When omitted, it will default to your own team, which is generally the desired behavior.

The returned value is a boolean that depends on the success of the operation. True means that the gift was sent, false means that an error occured.
If the gift is not sent successfully, it is recommended not to take away the item from the local player.

The out parameter is also optional, but it provides the uniquely generated ID for the gift you just sent. Keeping this around can help debugging, or identifying refunded gifts.

## Rejecting a Gift

If you receive a gift that you cannot or will not process properly in the current game, 3 options are available to you.

1: Refunding that gift.

```cs
public bool RefundGift(Gift gift); // Definition
var result = _service.RefundGift(unwantedGift); // Usage
```

When refunding a gift, that gift will be sent back to the original sender, with the flag `IsRefund` now set to `true`, so they know it is not a new gift, but a refund of a gift they originally sent.
It is then up to the original sender client to decide what to do with it. Typically, they would simply give back the gifted item to the player.

2: Selling the gift
The gift carries a value in Archipelago currency, which can be added to the EnergyLink for everyone to use, if your game supports interacting with the EnergyLink.

3: Ignoring that gift completely. This should be a last resort, as the item will then be lost forever.