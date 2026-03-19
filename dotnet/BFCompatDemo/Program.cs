using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

builder.Services.AddSingleton<IBotFrameworkHttpAdapter>(provider =>
    new CloudAdapter(
        provider.GetRequiredService<BotFrameworkAuthentication>(),
        provider.GetRequiredService<ILogger<CloudAdapter>>()));

builder.Services.AddTransient<IBot, EchoBot>();

var app = builder.Build();

app.MapPost("/api/messages", (IBotFrameworkHttpAdapter adapter, IBot bot, HttpRequest request, HttpResponse response)
    => adapter.ProcessAsync(request, response, bot));

app.Run();
