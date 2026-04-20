import { MessageActivity } from '@microsoft/teams.api';
import { App } from '@microsoft/teams.apps';
import { AdaptiveCard, CodeBlock } from '@microsoft/teams.cards';
import * as endpoints from '@microsoft/teams.graph-endpoints';
import { ConsoleLogger } from '@microsoft/teams.common';


const app = new App({
  oauth: {
    defaultConnectionName: 'graph'
  },
});

app.message('/signout', async ({ send, signout, isSignedIn }) => {
  if (!isSignedIn) return;
  await signout(); // call signout for your auth connection...
  await send('you have been signed out!');
});


app.on('message', async ({ log, signin, isSignedIn, userGraph, appGraph, send, activity }) => {
  if (!isSignedIn) {
    await signin();
    return;
  }

  const me = await userGraph.call(endpoints.me.get);

  await send(
    new MessageActivity(`hello ${me.displayName} 👋!`)
    .addCard(
      'adaptive',
      new AdaptiveCard(
        new CodeBlock({
          codeSnippet: JSON.stringify(me, null, 2),
        })
      )
    )
  );

  const me2 = await appGraph.call(endpoints.users.get, {
    "user-id": activity.recipient.aadObjectId ?? ''
  })

  await send(
    new MessageActivity(`hello 2 ${me.displayName} 👋!`)
    .addCard(
      'adaptive',
      new AdaptiveCard(
        new CodeBlock({
          codeSnippet: JSON.stringify(me2, null, 2),
        })
      )
    )
  );
});

app.event('signin', async ({ send, userGraph }) => {
  const me = await userGraph.call(endpoints.me.get);

  await send(
    new MessageActivity(`hello ${me.displayName} 👋!`).addCard(
      'adaptive',
      new AdaptiveCard(
        new CodeBlock({
          codeSnippet: JSON.stringify(me, null, 2),
        })
      )
    )
  );
});

app.on('signin.failure', async ({ activity, log, send }) => {
  const { code, message } = activity.value;
  log.error(`sign-in failed: ${code} - ${message}`);
  await send('Sign-in failed.');
});

app.start(process.env.PORT || 3978).catch(console.error);