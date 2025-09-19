
#:sdk Microsoft.NET.Sdk.Web
#:package Microsoft.Teams.Plugins.AspNetCore@2.*

using Microsoft.Teams.Plugins.AspNetCore.Extensions;
using Microsoft.Teams.Apps.Activities;

var builder = WebApplication.CreateBuilder(args);
builder.AddTeams();
var webApp = builder.Build();

var teamsApp = webApp.UseTeams();

teamsApp.OnMessage(async context =>
{
    await context.Send("Echo: " + context.Activity.Text);
});

webApp.Run();
