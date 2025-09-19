using Microsoft.Teams.AI;
using Microsoft.Teams.AI.Annotations;
using Microsoft.Teams.Plugins.External.McpClient;

namespace Spark.McpClient.WithDI;

[Prompt]
[Prompt.Description("helpful assistant")]
[Prompt.Instructions(
    "You are a helpful assistant that can help answer questions using Microsoft docs.",
    "You MUST use tool calls to do all your work."
)]
public class DocsPrompt(McpClientPlugin mcpClientPlugin)
{
    [ChatPlugin]
    public readonly IChatPlugin Plugin = mcpClientPlugin;
}