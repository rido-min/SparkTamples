import { ChatPrompt } from '@microsoft/teams.ai';
import { App } from '@microsoft/teams.apps';
import { ConsoleLogger } from '@microsoft/teams.common';
import { McpClientPlugin } from '@microsoft/teams.mcpclient';
import { OpenAIChatModel } from '@microsoft/teams.openai';

const app = new App();

const logger = new ConsoleLogger('mcp-client', { level: 'debug' });
const prompt = new ChatPrompt(
  {
    instructions: 'You are a helpful assistant. You MUST use tool calls to do all your work.',
    model: new OpenAIChatModel({
      model: 'gpt-4o-mini',
      apiKey: process.env.OPENAI_API_KEY,
    }),
    logger
  },
  [new McpClientPlugin()],
)
  .usePlugin('mcpClient', {
    url: 'https://learn.microsoft.com/api/mcp',
  });

app.on('message', async ({ send, activity }) => {
  const result = await prompt.send(activity.text);
  if (result.content) {
    await send(result.content);
  }
});

await app.start();

