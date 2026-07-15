# Providers

Compute providers connect PodPilot to GPU clouds (e.g. RunPod). AI providers connect inference backends used by routing and the gateway.

## Compute providers

Endpoints under `/api/v1/providers`:

- `POST /api/v1/providers` — register (name, type, API key)
- `GET /api/v1/providers` — list for current org
- `POST /api/v1/providers/{id}/validate` — credential check
- `GET /api/v1/providers/{id}/regions|gpus|templates|health`

CLI:

```bash
podpilot provider add --name runpod-prod --type RunPod --display-name "RunPod" --api-key '***'
```

API keys are encrypted at rest and never returned in responses.

## AI providers

Managed via `/api/v1/ai/providers` — OpenAI, Anthropic, Ollama-on-pod, and other kinds listed by `/api/v1/ai/provider-kinds`.

## Adding a new compute vendor

Implement `IComputeProvider` / factory registration in **Infrastructure** only. Handlers call `IProviderService` — do not reference vendor SDKs from Application.
