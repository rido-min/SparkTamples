using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using OpenAI;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

builder.Services.AddSingleton<IBotFrameworkHttpAdapter>(provider =>
    new CloudAdapter(
        provider.GetRequiredService<BotFrameworkAuthentication>(),
        provider.GetRequiredService<ILogger<CloudAdapter>>()));
builder.Services.AddTransient<IBot, AzureOpenAIAgentBot>();

var app = builder.Build();

app.MapPost("/api/messages", async (IBotFrameworkHttpAdapter adapter, IBot bot, HttpRequest request, HttpResponse response) =>
    await adapter.ProcessAsync(request, response, bot));

app.Run();

class AzureOpenAIAgentBot : ActivityHandler
{
    AIAgent agent;
    public AzureOpenAIAgentBot()
    {
        agent = new AzureOpenAIClient(
            new Uri("https://rido-aoai.openai.azure.com"), 
                    new ApiKeyCredential(Environment.GetEnvironmentVariable("AZURE_OpenAI_KEY")!))
            .GetChatClient("gpt-4o-mini")
            .CreateAIAgent(
                instructions: "You are good at telling jokes.", 
                name: "Joker");
    }


    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        var reply = await agent!.RunAsync(turnContext.Activity.Text, cancellationToken: cancellationToken);
        await turnContext.SendActivityAsync(MessageFactory.Text(reply.Text), cancellationToken);
    }
}