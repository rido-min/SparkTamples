using Microsoft.Teams.Api;
using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Api.Cards;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Plugins.AspNetCore.Extensions;


var builder = WebApplication.CreateBuilder(args);
builder.AddTeams();
var webApp = builder.Build();
var teamsApp = webApp.UseTeams();

teamsApp.OnMessage(async context =>
{
    var message = new MessageActivity("hi Suggested Actions")
      .WithSuggestedActions(new SuggestedActions()
        {
            To = [context.Activity.From.Id],
            Actions = [
                new Microsoft.Teams.Api.Cards.Action(ActionType.IMBack) {
                    Title = "Thank you!",
                    Value = "Thank you very much!"
                    }
            ]
        });
    await context.Send(message);
    throw new Exception("test exception");
});

webApp.Run("http://localhost:3978");
