# OpenAI SDK → PodPilot gateway

Use the official OpenAI client against PodPilot.

## Python

```python
from openai import OpenAI

client = OpenAI(
    base_url="http://localhost:5000/v1",
    api_key="pp_gw_YOUR_GATEWAY_KEY",
)

completion = client.chat.completions.create(
    model="llama3:latest",
    messages=[{"role": "user", "content": "Hello from PodPilot"}],
)
print(completion.choices[0].message.content)
```

## Node.js

```javascript
import OpenAI from 'openai';

const client = new OpenAI({
  baseURL: 'http://localhost:5000/v1',
  apiKey: process.env.PODPILOT_GATEWAY_KEY,
});

const completion = await client.chat.completions.create({
  model: 'llama3:latest',
  messages: [{ role: 'user', content: 'Hello from PodPilot' }],
});
console.log(completion.choices[0].message.content);
```

Ensure a gateway route exists for the model name.
