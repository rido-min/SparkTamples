using Json.Schema.Generation.Intents;
using Microsoft.Graph.Models;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Activities.Invokes;
using Microsoft.Teams.Apps.Events;
using Microsoft.Teams.Apps.Extensions;
using Microsoft.Teams.Common.Logging;
using Microsoft.Teams.Plugins.AspNetCore.Extensions;
using SparkSSO;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

var appBuilder = App.Builder()
    .AddLogger(new ConsoleLogger(level: Microsoft.Teams.Common.Logging.LogLevel.Info));
    

builder.AddTeams(appBuilder);

var app = builder.Build();
var teams = app.UseTeams();

teams.Use(async context =>
{
    var start = DateTime.UtcNow;
    try
    {
        await context.Next();
    }
    catch
    {
        context.Log.Error("error occurred during activity processing");
    }
    context.Log.Debug($"request took {(DateTime.UtcNow - start).TotalMilliseconds}ms");
});

teams.OnMessage("--signout", async (context, cancellationToken) =>
{
    await context.SignOut("gh",cancellationToken: cancellationToken); // call `SignOut()` for your auth connection...
    await context.Send("you have been signed out from GH", cancellationToken);

    await context.SignOut("sso", cancellationToken: cancellationToken); // call `SignOut()` for your auth connection...
    await context.Send("you have been signed out from Graph", cancellationToken);
});

teams.OnMessage(async (context, cancellationToken) =>
{
    var signRes = await context.SignIn(new OAuthOptions()
    {
        // Customize the OAuth card text (only applies to OAuth flow, not SSO)
        OAuthCardText = "Sign in to your account",
        SignInButtonText = "Sign In into GitHub",
        ConnectionName = "gh" // this should match the name of the auth connection you set up in the Azure portal
    }, cancellationToken); // call `SignIn() for your auth connection...
    Console.WriteLine(signRes);

    if (!string.IsNullOrEmpty(signRes))
    {
        var gitHubClient = new GitHubSimpleClient(signRes);
        var userInfo = await gitHubClient.GetUserInfoAsync();
        await context.Send($"user signed in GH! {userInfo}", cancellationToken);
    }

    var signRes2 = await context.SignIn(new OAuthOptions()
    {
        // Customize the OAuth card text (only applies to OAuth flow, not SSO)
        OAuthCardText = "Sign in to your account",
        SignInButtonText = "Sign In into Entra",
        ConnectionName = "sso" // this should match the name of the auth connection you set up in the Azure portal
    }, cancellationToken); // call `SignIn() for your auth connection...
    Console.WriteLine(signRes);



    if (!string.IsNullOrEmpty(signRes2))
    {
        var graphClient = new SimpleGraphClient(signRes2);
        User meInfo = await graphClient.GetMeAsync();
        await context.Send($"user signed in Graph! {meInfo.JobTitle}", cancellationToken);
    }
    
    else
    {
        var graphClient = new SimpleGraphClient(context.UserGraphToken?.ToString()!);
        User meInfo = await graphClient.GetMeAsync();
        await context.Send($"user is already signed in! {meInfo.JobTitle}", cancellationToken);
    }
});

teams.OnSignIn(async (_, @event, cancellationToken) =>
{
    var token = @event.Token;
    var context = @event.Context;

    var graphClient = new SimpleGraphClient(context.UserGraphToken?.ToString()!);
    User meInfo = await graphClient.GetMeAsync();
    await context.Send($"user is already signed in! {meInfo.JobTitle}", cancellationToken);
});

teams.OnSignInFailure(async (context, cancellationToken) =>
{
    var failure = context.Activity.Value;
    context.Log.Error($"sign-in failed: {failure?.Code} - {failure?.Message}");
    await context.Send("Sign-in failed.", cancellationToken);
});

app.Run();