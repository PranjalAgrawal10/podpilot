# API reference

Base URL default: `http://localhost:5000`

Responses use `ApiResponse<T>`:

```json
{ "success": true, "data": { }, "message": null, "errors": null, "correlationId": "..." }
```

## Auth

| Method | Path | Auth |
|--------|------|------|
| POST | `/api/v1/auth/register` | Anonymous |
| POST | `/api/v1/auth/login` | Anonymous |
| POST | `/api/v1/auth/refresh` | Body refresh token |
| POST | `/api/v1/auth/logout` | JWT |

## Core resources (JWT)

| Area | Base path |
|------|-----------|
| Organizations | `/api/v1/organizations` |
| Providers | `/api/v1/providers` |
| Pods | `/api/v1/pods` |
| Models | `/api/v1/models` |
| Gateway admin | `/api/v1/gateway` |
| Routing | `/api/v1/routing`, `/api/v1/ai/routing-policies` |
| AI providers | `/api/v1/ai/providers` |
| Plugins | `/api/v1/plugins` |
| MCP | `/api/v1/mcp` |
| Observability | `/api/v1/metrics`, `/api/v1/cost`, … |
| Health | `/api/v1/health` |

## Inference gateway (API key)

| Method | Path |
|--------|------|
| POST | `/v1/chat/completions` |
| POST | `/v1/responses` |
| POST | `/v1/messages` |
| GET | `/v1/models` |

Interactive OpenAPI: [openapi/README.md](openapi/README.md) · Postman: [postman/PodPilot.postman_collection.json](postman/PodPilot.postman_collection.json)
