#!/usr/bin/dotnet run

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#:sdk Microsoft.NET.Sdk.Web

#:package Microsoft.Teams.Plugins.AspNetCore@2.0.5

using Microsoft.Teams.Plugins.AspNetCore.Extensions;
using Microsoft.Teams.Apps.Activities;

var builder = WebApplication.CreateBuilder(args);
builder.AddTeams();
var webApp = builder.Build();

var teamsApp = webApp.UseTeams();

teamsApp.OnMessage(async context =>
{
    var a = context.Activity;
    await context.Send("Echo from dotnet with ❤️: " + context.Activity.Text);
});

teamsApp.OnTyping(async context =>
{
    var a = context.Activity;
    await context.Send("I see you're typing...");
});

webApp.Run("http://localhost:3978");
