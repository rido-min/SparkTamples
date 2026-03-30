using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Teams.Bot.Apps;
using Microsoft.Teams.Bot.Core.Schema;

class EchoBot(TeamsBotApplication teamsApp) : ActivityHandler
{
    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        
        var replyText = $"TM Echo: {turnContext.Activity.Text}";
        var activity = MessageFactory.Text(replyText, replyText);

        // Targeted Messages using BF Activity object
        activity.Recipient = turnContext.Activity.From;
        activity.Recipient.Properties.Add("isTargeted", true); // enable targeted messages
        var res = await turnContext.SendActivityAsync(activity, cancellationToken);
        turnContext.Activity.From.Properties.Remove("isTargeted"); // disable targeted messages

        // Targeted Messages using the new API
        await teamsApp.SendActivityAsync(CoreActivity.CreateBuilder()
            .WithServiceUrl(turnContext.Activity.ServiceUrl)
            .WithConversation(new Conversation { Id = turnContext.Activity.Conversation.Id })
            .WithRecipient(new Microsoft.Teams.Bot.Core.Schema.ConversationAccount { Id = turnContext.Activity.From.Id }, isTargeted: true)
            .WithFrom(new Microsoft.Teams.Bot.Core.Schema.ConversationAccount { Id = turnContext.Activity.Recipient.Id })
            .WithProperty("text", "TM using new API")
            .Build(), cancellationToken);

        // Add reactions using the new API
        await teamsApp.ConversationClient.AddReactionAsync(
            turnContext.Activity.Conversation.Id, 
            turnContext.Activity.Id, 
            "guitar", 
            new Uri(turnContext.Activity.ServiceUrl));

        await turnContext.SendActivityAsync("the end", cancellationToken: cancellationToken);
    }
}