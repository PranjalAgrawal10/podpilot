# Roo Code → PodPilot gateway

Roo Code supports OpenAI-compatible providers — point it at PodPilot.

## Configuration

1. PodPilot → Gateway → create API key.
2. PodPilot → Gateway → Routes → map model → GPU pod.
3. In Roo Code provider settings:

```text
Provider: OpenAI Compatible
Base URL: http://localhost:5000/v1
API Key:  <gateway key>
Model:    llama3:latest
```

Use your production API host instead of `localhost` when deployed.

## Tip

Create a dedicated gateway key named `roo-code` with rate limits appropriate for agent loops.
