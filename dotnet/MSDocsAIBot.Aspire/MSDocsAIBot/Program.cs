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
using System.Text.Json;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;
using Microsoft.Teams.Apps.Schema.Entities;

var chatHistories = new ConcurrentDictionary<string, List<ChatMessage>>();

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenTelemetry()
    .UseAzureMonitor()
    .WithTracing(t => t
        .AddSource("Experimental.Microsoft.Extensions.AI")
        .AddSource("ModelContextProtocol")
        .AddSource("OpenAI.*"))
    //.AddConsoleExporter())
    .WithMetrics(m => m
        .AddMeter("Experimental.Microsoft.Extensions.AI")
        .AddMeter("ModelContextProtocol")
        .AddMeter("OpenAI.*"));
        //.AddConsoleExporter());

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

var teamsApp = webApp.UseTeams();

teamsApp.OnMessage("--diag", async (ctx, ct) => {

    string diagInfo = $"version `v: {TeamsBotApplication.Version}` \r\n" +
                      $"cid `{ctx.Activity.Conversation?.Id}` \r\n" +
                      $"aid `{ctx.Activity.Id}` \r\n" +
                      $"from `{ctx.Activity.From?.Id}` \r\n";

    var diagMsg = TeamsActivity.CreateBuilder()
        .WithText(diagInfo, TextFormats.Markdown)
        .Build();

    await ctx.SendActivityAsync(diagMsg);
});

teamsApp.OnMessage(async (context, ct) =>
{
    ArgumentNullException.ThrowIfNull(context.Activity);
    ArgumentNullException.ThrowIfNull(context.Activity.Conversation);
    ArgumentNullException.ThrowIfNull(context.Activity.Conversation.Id);

    if (context.Activity.TextWithoutMentions == "--diag") return;

    await context.Typing(string.Empty, ct);

    var conversationId = context.Activity.Conversation.Id;
    var history = chatHistories.GetOrAdd(conversationId, _ => []);

    lock (history)
    {
        history.Add(new ChatMessage(ChatRole.User, context.Activity.Text));
    }

    var (responseText, citations) = await GetChatResponseAsync(history);

    var responseMsg = TeamsActivity.CreateBuilder()
        .WithText(responseText, TextFormats.Markdown)
        .AddMention(context.Activity?.From!)
        .Build();

    responseMsg.AddAIGenerated();

    for (int i = 0; i < citations.Count; i++)
    {
        var citation = citations[i];
        var abstract_ = citation.Content.Length > 400 ? citation.Content[..200] + "..." : citation.Content;
        responseMsg.AddCitation(i + 1, new CitationAppearance() { Name = citation.Title, Url = new Uri(citation.Url), Abstract = abstract_, Icon = CitationIcon.Text });
    }

    await context.Send(responseMsg, ct);
});

webApp.Run();

async Task<(string ResponseText, List<(string Title, string Url, string Content)> Citations)> GetChatResponseAsync(List<ChatMessage> history)
{
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

    var citations = response.Messages
        .SelectMany(m => m.Contents.OfType<FunctionResultContent>())
        .Where(frc => frc.Result is not null)
        .SelectMany(frc =>
        {
            try
            {
                var json = JsonSerializer.Deserialize<JsonElement>(frc.Result!.ToString()!);
                if (json.TryGetProperty("structuredContent", out var sc) &&
                    sc.TryGetProperty("results", out var results))
                {
                    return results.EnumerateArray()
                        .Where(r => r.TryGetProperty("contentUrl", out _))
                        .Select(r => (
                            Title: r.GetProperty("title").GetString() ?? "",
                            Url: r.GetProperty("contentUrl").GetString() ?? "",
                            Content: r.TryGetProperty("content", out var c) ? c.GetString() ?? "" : ""
                        ));
                }
            }
            catch { }
            return [];
        })
        .DistinctBy(c => c.Url)
        .Take(5).ToList();

    var responseText = response.Text;

    for (int i = 1; i < citations.Count; i++)
    {
        responseText += $"[{i}] ";
    }

    return (responseText, citations);
}

