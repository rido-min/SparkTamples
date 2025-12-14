// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Bot.Core;
using Microsoft.Bot.Core.Hosting;
using Microsoft.Bot.Core.Schema;

WebApplicationBuilder webAppBuilder = WebApplication.CreateSlimBuilder(args);
webAppBuilder.Services.AddOpenTelemetry().UseAzureMonitor();
webAppBuilder.Services.AddBotApplication<BotApplication>();
WebApplication webApp = webAppBuilder.Build();
BotApplication botApp = webApp.UseBotApplication<BotApplication>();


botApp.OnActivity = async (activity, cancellationToken) =>
{
    string replyText = $"Hello <3 <strong><at>{activity.From.Name}</at></strong>";  //$"CoreBot running on SDK `{BotApplication.Version}`. <br />";
    replyText += $"<br /> You sent: `{activity.Text}` in activity of type `{activity.Type}`.";
    CoreActivity replyActivity = activity.CreateReplyMessageActivity(replyText);
    await botApp.SendActivityAsync(replyActivity, cancellationToken);
};

webApp.Run();
