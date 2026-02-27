import { App } from '@microsoft/teams.apps'
import { MessageActivity } from '@microsoft/teams.api'

const app = new App()

app.message('unreact', async ({ api, activity }) => {
    const reaction = activity.text.split(' ')[1] 
    const reactionId = activity.text.split(' ')[2]
    await api.reactions.remove(
        activity.conversation.id,
        reactionId,
        reaction
    )
})

app.message('react', async ({ api, send, activity }) => {

    const reaction = activity.text.split(' ')[1]
    await api.reactions.add(
        activity.conversation.id,
        activity.id,
        reaction
    )

    await send(new MessageActivity('I added a reaction to this message!')
        .withSuggestedActions({
            to: [activity.from.id],
            actions: [
                {
                    type: 'imBack',
                    title: `unreact ${reaction} ${activity.id}`,
                    value: `unreact ${reaction} ${activity.id}`
                }
            ]
        }))
})

app.on('message', async ({ api, send, activity }) => {
    await send('Hello! I\'m a bot that demonstrates how to use reactions in Microsoft Teams. ')
    
    await api.reactions.add(
        activity.conversation.id,
        activity.id,
        'launch'
    )

    await send('Type react followed by a reaction name to add a reaction, or unreact followed by a reaction name and message id to remove a reaction. ' +
        'For example: react yes-tone3 or unreact launch ' + activity.id)
})

app.start()