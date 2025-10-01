using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OpenAI;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

builder.Services.AddSingleton<IBotFrameworkHttpAdapter>(provider =>
    new CloudAdapter(
        provider.GetRequiredService<BotFrameworkAuthentication>(),
        provider.GetRequiredService<ILogger<CloudAdapter>>()));
builder.Services.AddTransient<IBot, EchoBot>();

var app = builder.Build();

app.MapPost("/api/messages", async (IBotFrameworkHttpAdapter adapter, IBot bot, HttpRequest request, HttpResponse response) =>
    await adapter.ProcessAsync(request, response, bot));

app.Run();

class EchoBot : ActivityHandler
{
    IChatClient client;
    public EchoBot()
    {
         client = new ChatClientBuilder(new OpenAIClient(Environment.GetEnvironmentVariable("OPENAI_API_KEY")!).GetChatClient("gpt-4o").AsIChatClient())
        .UseFunctionInvocation()
        .UseLogging(LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(LogLevel.Information)))
        .Build();
    }
    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        await turnContext.SendActivityAsync(new Activity("typing"));

        var mcpClient = await McpClientFactory.CreateAsync(
        new SseClientTransport(new()
        {
            Endpoint = new Uri("https://learn.microsoft.com/api/mcp"),
            TransportMode = HttpTransportMode.AutoDetect,
            Name = "msdocs"
        }));

        var tools = await mcpClient.ListToolsAsync();
        Console.WriteLine("Tools Found: " + string.Join(", ", tools.Select(t => t.Name)));

        var chatOptions = new ChatOptions
        {
            AllowMultipleToolCalls = true,
            Instructions = "Use the following tools to answer the user's question. If you don't know the answer, use the 'Search Microsoft Docs' tool to find relevant information.",
            Tools = [.. tools]
        };

        ChatResponse response = await client.GetResponseAsync([new ChatMessage(ChatRole.User, turnContext.Activity.Text)], chatOptions);
        var toolsUsed = response.Messages.SelectMany(m => m.Contents.OfType<FunctionCallContent>());
        Console.WriteLine("Tools used " + toolsUsed.Count());
        await turnContext.SendActivityAsync(response.Text);
        var cc = response.RawRepresentation as OpenAI.Chat.ChatCompletion;
    }
}