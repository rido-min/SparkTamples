using Microsoft.Teams.AI.Models.OpenAI;
using Microsoft.Teams.AI.Prompts;
using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Plugins.AspNetCore.Extensions;
using Microsoft.Teams.Plugins.External.McpClient;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddTeams();
WebApplication webApp = builder.Build();

OpenAIChatPrompt prompt = new(
           new OpenAIChatModel(
               model: "gpt-4o",
               apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")!),
            new ChatPromptOptions()
                .WithDescription("helpful assistant")
                .WithInstructions(
                    "You are a helpful assistant that can help answer questions using Microsoft docs.",
                    "You MUST use tool calls to do all your work.")
                 );

prompt.Plugin(
    new McpClientPlugin()
        .UseMcpServer("https://learn.microsoft.com/api/mcp", 
            new McpClientPluginParams() 
            { 
                   HeadersFactory = () => new Dictionary<string, string>() 
                   { { "HEADER_KEY", "HEADER_VALUE" } } 
            })
        );

App app = webApp.UseTeams();
app.OnMessage(async context =>
{
    await context.Send(new TypingActivity());
    var result = await prompt.Send(context.Activity.Text);
    await context.Send(result.Content);
});
webApp.Run();




