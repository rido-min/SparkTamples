using Microsoft.Teams.AI.Models.OpenAI.Extensions;
using Microsoft.Teams.Plugins.AspNetCore.Extensions;
using Microsoft.Teams.Plugins.External.McpClient;

using Spark.McpClient.WithDI;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<TeamsController>().AddHttpContextAccessor();
builder.Services.AddSingleton((sp) => new 
    McpClientPlugin()
        .UseMcpServer("https://learn.microsoft.com/api/mcp", 
            new McpClientPluginParams() 
            { 
                HeadersFactory = () => new Dictionary<string, string>() 
                    { { "Authorization", "Bearer " + sp.GetRequiredService<IConfiguration>().GetValue<string>("Mcp:ApiKey") } } 
            }));
builder.AddTeams().AddOpenAI<DocsPrompt>();

var app = builder.Build();

app.UseTeams();
app.Run();




