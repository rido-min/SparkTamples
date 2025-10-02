import { App } from '@microsoft/teams.apps'

const app = new App()

app.on('message', async ({ send, activity }) => {
  await send(`echo from node 🚀"${activity.text}"`)
})

app.start().catch(console.error)