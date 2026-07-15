# Cursor → PodPilot gateway

Use PodPilot as an OpenAI-compatible model provider inside Cursor.

## Steps

1. Create a PodPilot **gateway API key** (Gateway → API keys).
2. Ensure a pod is running and a gateway **route** maps your model name to that pod.
3. In Cursor Settings → Models / OpenAI compatibility:

| Setting | Value |
|---------|-------|
| Base URL | `http://localhost:5000/v1` (or your deployed API + `/v1`) |
| API Key | Gateway key (`pp_gw_…`) |
| Model | Route model name, e.g. `llama3:latest` |

4. Select that model in the Cursor chat / agent picker.

## Notes

- User JWTs from `POST /api/v1/auth/login` are for the control plane, not `/v1` inference.
- For remote teams, terminate TLS at your ingress and set Cursor’s base URL to `https://api.example.com/v1`.
