// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using QuickStart;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Hosting.AspNetCore;
using Microsoft.Agents.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Linq;
using Microsoft.Agents.Builder.App;

AgentApplication myAgentApp = new AgentApplication(new AgentApplicationOptions(new MemoryStorage()));
myAgentApp.OnActivity("message", async (context, state, cancellationToken) =>
{
    var userMessage = context.Activity.Text;
    await context.SendActivityAsync($"You said: {userMessage}", cancellationToken: cancellationToken);
}, rank: RouteRank.Last);


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.AddAgentApplicationOptions();

builder.AddAgent<MyAgent>();

builder.Services.AddSingleton<IStorage, MemoryStorage>();

// Configure the HTTP request pipeline.

// Add AspNet token validation for Azure Bot Service and Entra.  Authentication is
// configured in the appsettings.json "TokenValidation" section.
builder.Services.AddControllers();
builder.Services.AddAgentAspNetAuthentication(builder.Configuration);

WebApplication app = builder.Build();

// Enable AspNet authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Microsoft Agents SDK Sample");

app.MapGet("api/test", (HttpContext context) => {
    if (context.User.Identities.Any())
    {
        return Results.Ok(context.User.Claims.Select(c => new { c.Type, c.Value }));
    }
    return Results.Ok("not authenticated");
}).RequireAuthorization();

// This receives incoming messages from Azure Bot Service or other SDK Agents
var incomingRoute = app.MapPost("/api/messages", async (HttpRequest request, HttpResponse response, IAgentHttpAdapter adapter, IAgent agent, CancellationToken cancellationToken) =>
{
    await adapter.ProcessAsync(request, response, agent, cancellationToken);
});


incomingRoute.RequireAuthorization();
// Hardcoded for brevity and ease of testing. 
// In production, this should be set in configuration.
app.Urls.Add($"http://localhost:3978");


app.Run();
