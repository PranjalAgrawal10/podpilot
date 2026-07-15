# Architecture

PodPilot is an AI infrastructure autopilot built with Clean Architecture and CQRS.

## Stack

| Layer | Technology |
|-------|------------|
| API | .NET 10, ASP.NET Core, MediatR, FluentValidation |
| Data | EF Core + MySQL, Redis |
| UI | React 19, TypeScript, Vite, React Query |
| Auth | JWT + org-scoped RBAC (`organization_id`, `organization_role`) |

## Layers

```
Api  →  Application  →  Domain
         ↑
Infrastructure (providers, EF, Redis)
Contracts (DTOs shared by Api / CLI / clients)
```

- **Domain** — entities and enums only
- **Application** — commands/queries, interfaces (`IComputeProvider`, `IPodProvider`, gateway)
- **Infrastructure** — RunPod and other SDKs, EF, Redis
- **Api** — thin controllers → MediatR
- **Contracts** — request/response DTOs

## Multi-tenancy

Every tenant resource is scoped by JWT `organization_id`. Handlers call access helpers and return `NotFound` when a resource is outside the org.

## Gateway

OpenAI- and Anthropic-compatible routes under `/v1/*` authenticate with gateway API keys and route to GPU pods / upstream AI providers.

## See also

- [installation.md](installation.md)
- [deployment.md](deployment.md)
- [security.md](security.md)
- [routing.md](routing.md)
