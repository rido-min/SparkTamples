#!/usr/bin/dotnet run

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#:sdk Microsoft.NET.Sdk.Web

#:package Microsoft.Bot.Core@0.0.1-alpha-0037-g26fbd3ec17

using Microsoft.Bot.Core;
using Microsoft.Bot.Core.Hosting;
using Microsoft.Bot.Core.Schema;

WebApplicationBuilder webAppBuilder = WebApplication.CreateSlimBuilder(args);
webAppBuilder.Services.AddBotApplication<BotApplication>();
WebApplication webApp = webAppBuilder.Build();
BotApplication botApp = webApp.UseBotApplication<BotApplication>();


botApp.OnActivity = async (activity, cancellationToken) =>
{
 
    string replyText = $"You sent: `{activity.Text}` in activity of type `{activity.Type}`.";
    CoreActivity replyActivity = activity.CreateReplyMessageActivity(replyText);
    await botApp.SendActivityAsync(replyActivity, cancellationToken);
};

webApp.Run();