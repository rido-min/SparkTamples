using FluentCards;
using Microsoft.Teams.Bot.Apps;
using Microsoft.Teams.Bot.Apps.Handlers;
using Microsoft.Teams.Bot.Apps.Schema;
using System.Text.Json;

WebApplicationBuilder webAppBuilder = WebApplication.CreateSlimBuilder(args);
webAppBuilder.Services.AddTeamsBotApplication();
WebApplication webApp = webAppBuilder.Build();

TeamsBotApplication teamsApp = webApp.UseTeamsBotApplication();

teamsApp.OnMessage(async (context, ct) =>
{
    await context.SendTypingActivityAsync(ct);
    await context.SendActivityAsync($"You sent: `{context.Activity.Text}` in activity of type `{context.Activity.Type}`.", ct);

    var msgActivity = TeamsActivity.CreateBuilder()
        .WithText("This message will self-destruct in 10 seconds. ⏳")
        .WithProperty("selfDestructTimer", 10000) // Custom property to indicate the message should be deleted after 10 seconds
        .WithAdaptiveCardAttachment(JsonElement.Parse(AdaptiveCardBuilder.Create()
                .WithVersion(AdaptiveCardVersion.V1_5)  // auto-sets $schema URL
                .AddTextBlock(tb => tb
                    .WithText("Hello, FluentCards!")
                    .WithSize(TextSize.Large)
                    .WithWeight(TextWeight.Bolder)
                    .WithWrap(true))
                .AddTextBlock(tb => tb
                    .WithText("This card was built with a fluent interface.")
                    .WithColor(TextColor.Accent))
                .Build().ToJson()))
        .Build();
    await context.SendActivityAsync(msgActivity, ct);
});

webApp.Run();