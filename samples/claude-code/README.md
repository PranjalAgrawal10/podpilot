# Claude Code → PodPilot gateway

Point Claude Code at PodPilot’s OpenAI-compatible gateway.

## 1. Create a gateway API key

In the PodPilot UI (Gateway → API keys) or:

```http
POST /api/v1/gateway/api-keys
Authorization: Bearer <user-jwt>
{ "name": "claude-code", "isPersonal": true }
```

Save the returned key once.

## 2. Configure Claude Code

Set the OpenAI-compatible base URL and key (exact env var names depend on your Claude Code version):

```bash
export OPENAI_BASE_URL=http://localhost:5000/v1
export OPENAI_API_KEY=pp_gw_***   # gateway key
```

Or in Claude Code settings / `.claude` config, set the custom provider endpoint to `http://localhost:5000/v1` with the gateway key.

## 3. Ensure a route exists

Create a gateway route for your model (e.g. `llama3:latest` → running pod) so chat completions resolve.

## Smoke test

```bash
curl http://localhost:5000/v1/chat/completions \
  -H "Authorization: Bearer $OPENAI_API_KEY" \
  -H "Content-Type: application/json" \
  -d '{"model":"llama3:latest","messages":[{"role":"user","content":"ping"}]}'
```
