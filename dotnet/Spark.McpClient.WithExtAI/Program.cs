using Microsoft.Teams.Plugins.AspNetCore.Extensions;
using Microsoft.Teams.Apps.Activities;
using ModelContextProtocol.Client;
using Microsoft.Extensions.AI;
using OpenAI;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

var builder = WebApplication.CreateBuilder(args);
builder.AddTeams();
var webApp = builder.Build();

IChatClient client =
    new ChatClientBuilder(new OpenAIClient(Environment.GetEnvironmentVariable("OPENAI_API_KEY")!).GetChatClient("gpt-4o").AsIChatClient())
    .UseFunctionInvocation()
    .Build();

var mcpClient = await McpClientFactory.CreateAsync(new SseClientTransport(new SseClientTransportOptions() { Endpoint = new Uri("https://learn.microsoft.com/api/mcp") }));
var tools = await mcpClient.ListToolsAsync();

var chatOptions = new ChatOptions
{
    Tools = [.. tools]
};

List<ChatMessage> chatHistory = [new(ChatRole.System, """
    You are a helpful assistant that can help answer questions using Microsoft docs.
    You MUST use tool calls to do all your work.
    """)];

var teamsApp = webApp.UseTeams();

teamsApp.OnMessage(async context =>
{
    chatHistory.Add(new ChatMessage(ChatRole.User, context.Activity.Text));
    ChatResponse response = await client.GetResponseAsync(chatHistory, chatOptions);
    await context.Send(response.Text);
});

webApp.Run();

