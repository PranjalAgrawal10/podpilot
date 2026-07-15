# Cline → PodPilot gateway

Configure Cline to call PodPilot’s OpenAI-compatible API.

## Settings

In Cline’s OpenAI-compatible / API provider settings:

| Field | Value |
|-------|-------|
| API Provider | OpenAI Compatible |
| Base URL | `http://localhost:5000/v1` |
| API Key | PodPilot gateway API key |
| Model ID | `llama3:latest` (must match a gateway route) |

## Verify

```bash
curl -s http://localhost:5000/v1/models \
  -H "Authorization: Bearer pp_gw_YOUR_GATEWAY_KEY"
```

If models are empty, create a gateway route or pull a model onto a running pod first.
