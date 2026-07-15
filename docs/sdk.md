# SDKs

Minimal official clients live under `sdk/`:

| Language | Path |
|----------|------|
| .NET | `sdk/dotnet/PodPilot.Sdk` |
| TypeScript | `sdk/typescript` |
| Python | `sdk/python` |
| Go | `sdk/go` |
| Java | `sdk/java` |

Each package README includes a working login → health → list pods → gateway stats example.

## Common pattern

1. Construct client with `PODPILOT_API_URL` (default `http://localhost:5000`)
2. `login(email, password)` → store bearer token
3. Call authenticated endpoints; treat non-`success` envelopes as errors

## Inference from apps

For OpenAI/Anthropic-compatible IDEs and SDKs, use the **gateway** base URL (`/v1`) with a gateway API key — see [samples/](../samples/) and [sdk.md examples in language READMEs](../sdk/dotnet/README.md).
