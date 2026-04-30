

using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Handlers;
using Microsoft.Teams.Apps.Schema;

var builder = WebApplication.CreateBuilder(args);
builder.AddTeams();
var webApp = builder.Build();
var teamsApp = webApp.UseTeams();

teamsApp.OnMessage(async (context, ct) =>
{
    ArgumentNullException.ThrowIfNull(context.Activity);
    ArgumentNullException.ThrowIfNull(context.Activity.From);
    ArgumentNullException.ThrowIfNull(context.Activity.From.Id);

    var message = TeamsActivity.CreateBuilder()
        .WithText("hi Suggested Actions")
        .WithSuggestedActions(new SuggestedActions()
        {
            To = [context.Activity.From.Id],
            Actions = [
                new SuggestedAction(ActionType.IMBack, "Thank you!") {
                    Value = "Thank you very much!"
                    }
            ]
        })
        .Build();
    await context.Send(message, ct);
});

webApp.Run("http://localhost:3978");
