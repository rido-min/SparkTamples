using Microsoft.Teams.Api;
using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Api.Cards;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Plugins.AspNetCore.Extensions;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddTeams();
WebApplication webApp = builder.Build();
Microsoft.Teams.Apps.App teamsApp = webApp.UseTeams();

teamsApp.OnMessage(async (context, ct) =>
{
    ILogger logger = webApp.Services.GetRequiredService<ILogger>();
    logger.LogWarning("This is a warning log from the message handler.");

    MessageActivity message = new MessageActivity("hi Suggested Actions")
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
    await context.Send(message, ct);
});

webApp.Run("http://localhost:3978");
