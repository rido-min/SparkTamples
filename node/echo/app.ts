import { App } from '@microsoft/teams.apps'

const app = new App()

app.on('message', async ({ send, activity }) => {
  await send(`you said "${activity.text}"`)
})

app.start().catch(console.error)