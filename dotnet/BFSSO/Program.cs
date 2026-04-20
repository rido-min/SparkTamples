// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using BFSSO;
using BFSSO.Bots;
using BFSSO.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add necessary services to the container.
ConfigureServices(builder.Services);

var app = builder.Build();

app.MapPost("/api/messages", (IBotFrameworkHttpAdapter adapter, IBot bot, HttpRequest request, HttpResponse response)
    => adapter.ProcessAsync(request, response, bot));


app.Run();

void ConfigureServices(IServiceCollection services)
{
    services.AddAuthorization();
    // Add HTTP client and JSON configuration.
//    services.AddHttpClient().AddControllers().AddNewtonsoftJson();

    // Register the Bot Framework Adapter with error handling.
    services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

    // Register Bot Framework Authentication.
    services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

    // Register state management services. Consider using Scoped for better isolation.
    services.AddScoped<IStorage, MemoryStorage>(); // Consider replacing MemoryStorage with persistent storage for production.
    services.AddScoped<UserState>();
    services.AddScoped<ConversationState>();

    // Register the dialog to be used by the bot.
    services.AddSingleton<MainDialog>();

    // Register the bot as a transient service.
    services.AddTransient<IBot, TeamsBot>();
}


