# CLI

.NET console app: `src/PodPilot.Cli` (assembly name `podpilot`).

## Setup

```bash
export PODPILOT_API_URL=http://localhost:5000   # default if unset
dotnet run --project src/PodPilot.Cli -- --help
```

Credentials are stored at `~/.podpilot/credentials.json`.

## Commands

| Command | Description |
|---------|-------------|
| `login --email --password` | Authenticate; save tokens |
| `provider add ...` | Register compute provider |
| `pod create ...` | Create GPU pod |
| `model pull --pod-id --model` | Pull Ollama model |
| `gateway test` | Health + gateway stats |
| `status` | Health, pods, gateway summary |
| `logs [--pod-id] [--limit]` | Pod activity or gateway requests |

Global `--api-url` overrides `PODPILOT_API_URL` / saved URL.

## Examples

```bash
dotnet run --project src/PodPilot.Cli -- login --email you@example.com --password '***'

dotnet run --project src/PodPilot.Cli -- provider add \
  --name runpod --type RunPod --display-name RunPod --api-key '***'

dotnet run --project src/PodPilot.Cli -- gateway test
dotnet run --project src/PodPilot.Cli -- status
dotnet run --project src/PodPilot.Cli -- logs --limit 20
```
