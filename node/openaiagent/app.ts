import { Agent, hostedMcpTool, run } from '@openai/agents'
import { App } from '@microsoft/teams.apps'

const agent = new Agent({
    name: 'MCP Assistant',
    instructions: 'You must always use the MCP tools to answer questions.',
    tools: [
        hostedMcpTool({
            serverLabel: 'msdocs',
            serverUrl: 'https://learn.microsoft.com/api/mcp',
        }),
    ],
})

const app = new App()

app.on('message', async ({ send, activity }) => {
    send(`You said "${activity.text}", let me think...`)
    const result = await run(agent, activity.text);
    send(result.finalOutput!)
})

app.start()