using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

class EchoBot : ActivityHandler
{
    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        var t = new Activity("typing");
        var rest = await turnContext.SendActivityAsync(t);

        var replyText = $"Echo: {turnContext.Activity.Text}";
        var res = await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
        Console.WriteLine(res);
    }
}