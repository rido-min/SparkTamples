using Microsoft.Teams.Plugins.AspNetCore.Extensions;
using Microsoft.Teams.Apps.Activities;


var builder = WebApplication.CreateBuilder(args);
builder.AddTeams();
var webApp = builder.Build();

var teamsApp = webApp.UseTeams();

teamsApp.OnMessage(async context =>
{
    var a = context.Activity;
    await context.Send("Echo: " + context.Activity.Text);
    await context.Send(a.AddAIGenerated());
});

teamsApp.OnTyping(async context =>
{
    var a = context.Activity;
    await context.Send("I see you're typing...");
});

webApp.Run();
