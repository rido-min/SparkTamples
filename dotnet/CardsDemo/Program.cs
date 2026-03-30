//using Microsoft.Teams.Cards;
using AdaptiveCards;
using System.Text.Json;

//var card = new AdaptiveCard(
//    [
//        new TextBlock("Hello world")
//        {
//            Wrap = true,
//            Weight = new TextWeight("red")
//        }
//    ])
//{
//    Schema = "http://adaptivecards.io/schemas/adaptive-card.json",
//};

var card = new AdaptiveCard("http://adaptivecards.io/schemas/adaptive-card.json")
{
    Body =
    [
        new AdaptiveTextBlock("Hello world")
        {
            Wrap = true,
            Weight = AdaptiveTextWeight.Bolder
        }
    ]
};

Console.WriteLine(JsonSerializer.Serialize(card, new JsonSerializerOptions { WriteIndented = true }));