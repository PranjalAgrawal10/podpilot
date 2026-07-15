# Continue → PodPilot gateway

[Continue](https://continue.dev) can use an OpenAI-compatible endpoint.

## Example `config.json` / YAML snippet

```json
{
  "models": [
    {
      "title": "PodPilot",
      "provider": "openai",
      "model": "llama3:latest",
      "apiBase": "http://localhost:5000/v1",
      "apiKey": "pp_gw_YOUR_GATEWAY_KEY"
    }
  ]
}
```

1. Create the gateway key in PodPilot.
2. Map `llama3:latest` (or your model) via Gateway → Routes.
3. Restart Continue / reload the window and select **PodPilot**.
