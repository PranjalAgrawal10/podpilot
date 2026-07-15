# Anthropic SDK → PodPilot gateway

PodPilot exposes an Anthropic-compatible messages endpoint at `/v1/messages`.

## Python

```python
import anthropic

client = anthropic.Anthropic(
    base_url="http://localhost:5000",
    api_key="pp_gw_YOUR_GATEWAY_KEY",
)

message = client.messages.create(
    model="llama3:latest",
    max_tokens=256,
    messages=[{"role": "user", "content": "Hello from PodPilot"}],
)
print(message.content)
```

Some SDK versions append `/v1/messages` to `base_url`. If you set `base_url` to `http://localhost:5000/v1`, confirm the final path is `/v1/messages` (not `/v1/v1/messages`).

## curl

```bash
curl http://localhost:5000/v1/messages \
  -H "Authorization: Bearer pp_gw_YOUR_GATEWAY_KEY" \
  -H "Content-Type: application/json" \
  -H "anthropic-version: 2023-06-01" \
  -d '{
    "model": "llama3:latest",
    "max_tokens": 256,
    "messages": [{"role": "user", "content": "Hello"}]
  }'
```
