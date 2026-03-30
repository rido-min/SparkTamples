using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Teams.Bot.Compat;

var builder = WebApplication.CreateBuilder(args);

builder.AddCompatAdapter();
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, CompatAdapter>();
builder.Services.AddTransient<IBot, EchoBot>();

var app = builder.Build();

app.MapPost("/api/messages", (IBotFrameworkHttpAdapter adapter, IBot bot, HttpRequest request, HttpResponse response)
    => adapter.ProcessAsync(request, response, bot));

app.Run();
