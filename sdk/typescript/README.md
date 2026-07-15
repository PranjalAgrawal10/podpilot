# PodPilot TypeScript SDK

Minimal fetch-based client for PodPilot.

## Install

```bash
cd sdk/typescript
npm install
npm run build
```

## Example

```typescript
import { PodPilotClient } from '@podpilot/sdk';

const client = new PodPilotClient({
  baseUrl: process.env.PODPILOT_API_URL ?? 'http://localhost:5000',
});

const auth = await client.login('you@example.com', 'your-password');
console.log('token expires in', auth.expiresIn, 's');

const health = await client.getHealth();
console.log('API health:', health.status);

const pods = await client.listPods();
for (const pod of pods) {
  console.log(pod.id, pod.name, pod.status);
}

const gateway = await client.getGatewayStats();
console.log('gateway errors', gateway.recentErrors, 'avg ms', gateway.averageLatencyMs);
```

Point the client at your deployment with `PODPILOT_API_URL`.
