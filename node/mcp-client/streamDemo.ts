import { App } from '@microsoft/teams.apps';
import OpenAI from 'openai';

const openai = new OpenAI({
    apiKey: process.env.OPENAI_API_KEY
});

const app = new App();
app.on('message', async ({ send, stream, activity }) => {
    
    send('Processing your request...');
    const llm = await openai.chat.completions.create({
        model: 'gpt-4o', 
        messages: [{ role: 'user', content: activity.text }],
        stream: true,
    });
    
    try {
        for await (const chunk of llm) {
            const content = chunk.choices[0]?.delta?.content || '';
            if (content) {
                process.stdout.write(content);
                stream.emit(content); // Emit content to the stream
            }
        }
        stream.close();
        console.log('\n'); // New line after streaming completes
    } catch (error) {
        console.error('Error streaming completion:', error);
    }
});

app.start();


