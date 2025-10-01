using Microsoft.Teams.Plugins.AspNetCore.Extensions;
using Microsoft.Teams.Apps.Activities;
using Json.More;

var builder = WebApplication.CreateBuilder(args);
builder.AddTeams();
var webApp = builder.Build();

var teamsApp = webApp.UseTeams();

teamsApp.OnMessage(async context =>
{
    var a = context.Activity;
    var a2 = a.ToInvoke();
    await context.Send("Echo: " + context.Activity.Text);
    await context.Send(a2.AddAIGenerated());
});

teamsApp.OnTyping(async context =>
{
    var a = context.Activity;
    var a2 = a.ToInvoke();
    Console.WriteLine(a2.ToJsonDocument().RootElement.ToJsonString());
    await context.Send("I see you're typing...");
});

webApp.Run();
