# Routing

PodPilot routes inference traffic across pods and AI providers using policies.

## GPU / gateway routes

Gateway routes map a model name to a GPU pod:

```http
POST /api/v1/gateway/routes
{ "gpuPodId": "...", "modelName": "llama3:latest", "isDefault": true }
```

Incoming `/v1/chat/completions` (gateway API key auth) selects a route, may wake the pod, then proxies to Ollama or an upstream provider.

## Smart AI routing

Endpoints under `/api/v1/routing` and `/api/v1/ai/routing-policies`:

- Read/update routing policy
- List models available for routing
- View history
- `POST /api/v1/routing/simulate` — dry-run a request

Use routing policies to prefer cost, latency, or capability when multiple AI providers are configured.

## Scheduler

Long-running or queued work is tracked via `/api/v1/scheduler` and `/api/v1` queue endpoints for fairness under load.
