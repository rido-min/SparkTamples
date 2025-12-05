using Microsoft.Teams.Apps;
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

webApp.Run("http://localhost:3978");
