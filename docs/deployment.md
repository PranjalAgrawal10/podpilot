# Deployment

## Options

| Path | Location |
|------|----------|
| Docker Compose | `docker-compose.yml` |
| Helm | `deploy/helm/podpilot/` |
| Azure Container Apps | `deploy/azure/` |
| AWS ECS Fargate | `deploy/aws/` |
| GCP Cloud Run | `deploy/gcp/` |
| K8s quickstart | `deploy/k8s/quickstart.md` |

## Required configuration

Always set:

- `ConnectionStrings__DefaultConnection` (MySQL)
- `ConnectionStrings__Redis` (Redis)
- `Jwt__Secret`

Compose ships Redis; MySQL may remain on the host (see compose comments).

## Images

Build:

```bash
docker build -f src/PodPilot.Api/Dockerfile -t podpilot-api .
docker build -f web/Dockerfile -t podpilot-web ./web
```

CI release workflow pushes placeholders to GHCR on `v*` tags — see `.github/workflows/release.yml`.

## Health

`GET /api/v1/health` — use for probes. Gateway readiness can be checked with `podpilot gateway test` after login.
