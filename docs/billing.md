# Billing & cost

PodPilot tracks inference and infrastructure cost signals for organizations.

## Observability cost APIs

| Path | Purpose |
|------|---------|
| `GET /api/v1/cost` | Cost breakdown |
| `GET /api/v1/analytics` | Usage analytics |
| `GET /api/v1/metrics` | Operational metrics |

Commercial / plan contracts live under `PodPilot.Contracts/Commercial` for future metering expansions.

## Practices

- Prefer idle policies and auto-shutdown on GPU pods to control spend
- Use routing policies to prefer cheaper providers when quality allows
- Export observability data (`/api/v1/observability/export`) for finance reconciliation

PodPilot itself does not process card payments in-core; connect your billing provider externally and attribute usage via cost APIs.
