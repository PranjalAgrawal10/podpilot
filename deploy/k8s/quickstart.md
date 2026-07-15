# Kubernetes Quickstart

Fastest path: Docker Compose locally, Helm for clusters.

## Local (Compose)

```bash
# Ensure MySQL is reachable on the host (or uncomment mysql in docker-compose.yml).
export JWT_SECRET='change-me-in-production'
export MYSQL_PASSWORD='podpilot_secret'

docker compose up --build -d
curl -s http://localhost:5000/api/v1/health
# Web UI: http://localhost:3000
```

MySQL is **optional** inside Compose — by default the API uses `host.docker.internal:3306`.

## Helm

```bash
helm upgrade --install podpilot deploy/helm/podpilot \
  --namespace podpilot --create-namespace \
  --set secrets.mysqlConnection='Server=mysql;Port=3306;Database=podpilot;User=podpilot;Password=***;' \
  --set secrets.jwtSecret='CHANGE_ME_LONG_RANDOM' \
  --set image.api.repository=ghcr.io/you/podpilot-api \
  --set image.api.tag=latest \
  --set image.web.repository=ghcr.io/you/podpilot-web \
  --set image.web.tag=latest
```

Helm charts deploy **API + web + Redis**. Bring your own MySQL (managed or in-cluster).

## Cloud shortcuts

| Platform | Guide |
|----------|-------|
| Azure Container Apps | [deploy/azure/README.md](../azure/README.md) |
| AWS ECS Fargate | [deploy/aws/README.md](../aws/README.md) |
| GCP Cloud Run | [deploy/gcp/README.md](../gcp/README.md) |

## Required env vars

| Variable | Required | Notes |
|----------|----------|-------|
| `ConnectionStrings__DefaultConnection` | Yes | MySQL |
| `ConnectionStrings__Redis` | Yes | Redis host:port |
| `Jwt__Secret` | Yes | Long random string |
| `Cors__AllowedOrigins__0` | Recommended | Web origin |
| `Swagger__Enabled` | Optional | `true` in non-dev if needed |
