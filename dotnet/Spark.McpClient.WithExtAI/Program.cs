using Microsoft.Extensions.AI;
using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Plugins.AspNetCore.Extensions;
using ModelContextProtocol.Client;
using OpenAI;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

IChatClient client =
    new ChatClientBuilder(new OpenAIClient(Environment.GetEnvironmentVariable("OPENAI_API_KEY")!).GetChatClient("gpt-4o").AsIChatClient())
    .UseFunctionInvocation()
    .UseLogging(LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(LogLevel.Information)))
    .Build();

var mcpClient = await McpClientFactory.CreateAsync(
    new SseClientTransport(new () { 
        Endpoint = new Uri("https://learn.microsoft.com/api/mcp"), 
        TransportMode = HttpTransportMode.AutoDetect, 
        Name = "msdocs" }));

var tools = await mcpClient.ListToolsAsync();
Console.WriteLine("Tools Found: " + string.Join(", ", tools.Select(t => t.Name)));

var chatOptions = new ChatOptions
{
    AllowMultipleToolCalls = true,
    Instructions = "Use the following tools to answer the user's question. If you don't know the answer, use the 'Search Microsoft Docs' tool to find relevant information.",
    Tools = [.. tools]
};



var builder = WebApplication.CreateBuilder(args);
builder.AddTeams();
var webApp = builder.Build();

var teamsApp = webApp.UseTeams();

teamsApp.OnMessage(async context =>
{
    await context.Send(new TypingActivity());
    ChatResponse response = await client.GetResponseAsync([new ChatMessage(ChatRole.User, context.Activity.Text)], chatOptions);
    await context.Send(response.Text);
    var cc = response.RawRepresentation as OpenAI.Chat.ChatCompletion;
});

webApp.Run();

