# Best practices

## Multi-tenancy

- Never pass `organizationId` from the client for authorization decisions
- Always invalidate React Query caches with org-scoped keys after mutations

## Providers & pods

- Validate credentials before creating pods at scale
- Set idle/wake policies early to avoid idle GPU burn
- Treat provider API keys as secrets — rotate via update/recreate flows

## Gateway

- Issue separate gateway keys per IDE / environment
- Set rate limits on keys
- Prefer default routes for common models; use routing policies for multi-provider failover

## Operations

- Keep `TreatWarningsAsErrors` green (`dotnet build` / `dotnet test`)
- Run `npm run build` for web changes
- Store MySQL/Redis/JWT secrets in a vault or orchestrator secret store — not in git

## Clients

- Prefer official SDKs under `sdk/` for typed envelope handling
- Surface `correlationId` from failed responses when opening support tickets
