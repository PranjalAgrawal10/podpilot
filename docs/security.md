# Security

## Authentication

- User JWT from `POST /api/v1/auth/login` (and refresh/logout)
- Gateway API keys for `/v1/*` inference endpoints
- Optional SSO / MFA flows under `/api/v1/auth/*` and Security APIs

## Authorization

- Org context from JWT claims only — never trust client-supplied `organizationId` for authorization
- Permissions enforced in MediatR handlers via `IOrganizationAuthorizationService`
- Cross-org access returns **404** (not 403)

## Secrets

| Kind | Storage |
|------|---------|
| Provider API keys | Encrypted at rest |
| Gateway keys | Hashed / rotated via `/api/v1/gateway/api-keys` |
| Org secrets | `/api/v1/secrets` |
| JWT signing key | `Jwt__Secret` env / secret store |

Never commit production secrets. Use `.env.example` as a template only.

## Hardening checklist

- Rotate `Jwt__Secret` and gateway keys regularly
- Disable Swagger in production unless intentionally exposed
- Restrict CORS origins
- Prefer managed MySQL/Redis with TLS
- Review audit logs (`/api/v1/audit`) and security dashboard
