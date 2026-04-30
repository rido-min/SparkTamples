using Microsoft.Extensions.AI;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Handlers;
using Microsoft.Teams.Apps.Schema;
using ModelContextProtocol.Client;
using OpenAI;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using System.Collections.Concurrent;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

var chatHistories = new ConcurrentDictionary<string, List<ChatMessage>>();

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenTelemetry()
    .UseAzureMonitor()
    .WithTracing(t => t
        .AddSource("Experimental.Microsoft.Extensions.AI")
        .AddSource("ModelContextProtocol")
        .AddSource("OpenAI.*")
        .AddConsoleExporter())
    .WithMetrics(m => m
        .AddMeter("Experimental.Microsoft.Extensions.AI")
        .AddMeter("ModelContextProtocol")
        .AddMeter("OpenAI.*")
        .AddConsoleExporter());

IChatClient client =
    new ChatClientBuilder(
        new OpenAIClient(Environment.GetEnvironmentVariable("OPENAI_API_KEY")!).GetChatClient("gpt-5.1").AsIChatClient())
            .UseFunctionInvocation()
            .UseOpenTelemetry(sourceName: "Experimental.Microsoft.Extensions.AI")
            //.UseLogging(LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(LogLevel.Information)))
            .Build();

var mcpClient = await McpClient.CreateAsync(
    new HttpClientTransport(new()
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

builder.AddTeams();
var webApp = builder.Build();

webApp.MapDefaultEndpoints();
var teamsApp = webApp.UseTeams();

teamsApp.OnMessage(async (context, ct) =>
{
    ArgumentNullException.ThrowIfNull(context.Activity);
    ArgumentNullException.ThrowIfNull(context.Activity.Conversation);
    ArgumentNullException.ThrowIfNull(context.Activity.Conversation.Id);

    await context.Typing(string.Empty, ct);

    var conversationId = context.Activity.Conversation.Id;
    var history = chatHistories.GetOrAdd(conversationId, _ => []);

    lock (history)
    {
        history.Add(new ChatMessage(ChatRole.User, context.Activity.Text));
    }

    List<ChatMessage> snapshot;
    lock (history)
    {
        snapshot = [.. history];
    }

    ChatResponse response = await client.GetResponseAsync(snapshot, chatOptions);

    lock (history)
    {
        history.AddRange(response.Messages);
    }

    var toolsUsed = response.Messages.SelectMany(m => m.Contents.OfType<FunctionCallContent>());
    Console.WriteLine("Tools used " + toolsUsed.Count());

    var responseMsg = TeamsActivity.CreateBuilder()
        .WithText($"{response.Text}", TextFormats.Markdown)
        .AddMention(context.Activity?.From!)
        .Build();

    await context.Send(responseMsg, ct);
});

webApp.Run();

