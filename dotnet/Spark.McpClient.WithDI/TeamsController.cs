using Microsoft.Teams.AI.Models.OpenAI;
using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Annotations;

namespace Spark.McpClient.WithDI;

[TeamsController]
public class TeamsController(Func<OpenAIChatPrompt> _promptFactory)
{
    [Message]
    public async Task OnMessage(IContext<MessageActivity> context)
    {
        await context.Send(new TypingActivity());
        var prompt = _promptFactory();
        var result = await prompt.Send(context.Activity.Text);
        await context.Send(result.Content);
    }
}

