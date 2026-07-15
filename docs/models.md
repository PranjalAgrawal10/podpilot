# Models

PodPilot manages Ollama models on GPU pods.

## Pull a model

```http
POST /api/v1/models/pull
Authorization: Bearer <jwt>
Content-Type: application/json

{ "podId": "<guid>", "model": "llama3:latest" }
```

CLI:

```bash
podpilot model pull --pod-id <guid> --model llama3:latest
```

## Other operations

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/v1/models` | List models (`?podId=`) |
| GET | `/api/v1/models/dashboard` | Dashboard metrics |
| POST | `/api/v1/models/{id}/default` | Set default |
| DELETE | `/api/v1/models/{id}` | Delete |
| GET | `/api/v1/models/downloads` | Download progress |
| GET | `/api/v1/models/health` | Health |

Pulled models can be exposed through gateway routes (`/api/v1/gateway/routes`) for OpenAI-compatible clients.
