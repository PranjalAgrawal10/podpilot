# Azure Container Apps

Deploy PodPilot API, web UI, and Redis to Azure Container Apps using the included Bicep template.

## Prerequisites

- Azure CLI (`az`)
- Existing MySQL Flexible Server (or other MySQL) reachable from the Container Apps environment
- Container images pushed to a registry ACA can pull (GHCR/ACR)

## Secrets / env vars

| Variable | Purpose |
|----------|---------|
| `mysqlConnectionString` | `ConnectionStrings__DefaultConnection` |
| `jwtSecret` | `Jwt__Secret` |
| Redis | In-cluster `podpilot-redis:6379` from the Bicep Redis app |

## Deploy

```bash
az group create --name podpilot-rg --location eastus

az deployment group create \
  --resource-group podpilot-rg \
  --template-file deploy/azure/container-apps.bicep \
  --parameters \
    mysqlConnectionString='Server=...;Database=podpilot;User=...;Password=...;' \
    jwtSecret='REPLACE_WITH_LONG_RANDOM_SECRET' \
    apiImage='ghcr.io/you/podpilot-api:latest' \
    webImage='ghcr.io/you/podpilot-web:latest'
```

## Sample Container App YAML (API excerpt)

Use this when configuring via Azure Portal / `az containerapp update`:

```yaml
properties:
  configuration:
    ingress:
      external: true
      targetPort: 8080
    secrets:
      - name: mysql-connection
        value: "<from-key-vault>"
      - name: jwt-secret
        value: "<from-key-vault>"
  template:
    containers:
      - name: api
        image: ghcr.io/podpilot/api:latest
        env:
          - name: ASPNETCORE_URLS
            value: http://+:8080
          - name: ConnectionStrings__DefaultConnection
            secretRef: mysql-connection
          - name: ConnectionStrings__Redis
            value: podpilot-redis:6379
          - name: Jwt__Secret
            secretRef: jwt-secret
```

After deploy, point the web app's API base URL at the API FQDN and set `Cors__AllowedOrigins__0` accordingly.
