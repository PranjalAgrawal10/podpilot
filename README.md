# PodPilot

**PodPilot** is an AI Infrastructure Autopilot that automatically manages GPU pods, AI models, and inference providers. This repository contains **Part 1** (authentication foundation), **Part 2** (multi-tenant organization management), **Part 3** (provider management abstraction), and **Part 4** (GPU pod management).

---

## Part 2 — Multi-Tenant Organizations

Part 2 transforms PodPilot into a **multi-tenant SaaS**. Every user belongs to one or more organizations, and all future resources (Pods, Models, Providers, Sessions) will be scoped to an organization.

### Multi-Tenant Design

```mermaid
flowchart LR
    User --> Member
    Member --> Organization
    Organization --> Invitation
    Member --> Role
    Role --> Permission
```

- **Organization** — tenant boundary with slug, owner, and default-org flag
- **OrganizationMember** — links users to organizations with a role and status
- **Invitation** — email-based onboarding with expiring tokens
- **Permission** — granular capabilities (e.g. `Organization.Read`, `Pod.Create`)
- **Role** — Owner, Admin, Developer, Viewer with seeded permission mappings

### Current Organization Context

Users may belong to multiple organizations. The **active organization** is persisted in JWT claims:

| Claim | Description |
|-------|-------------|
| `organization_id` | Currently selected organization |
| `organization_role` | User's role in that organization |

Switching organizations calls `POST /organizations/switch`, which re-issues JWT + refresh tokens with updated claims.

### Permission System

Permissions are defined in `PermissionNames` and mapped to roles via `RolePermissionMatrix`:

| Role | Capabilities |
|------|-------------|
| **Owner** | Full control — delete org, transfer ownership, manage all resources |
| **Admin** | Manage members, invitations, and settings (cannot delete org) |
| **Developer** | Create/manage pods, providers, models (cannot manage users) |
| **Viewer** | Read-only access to organization resources |

Authorization is enforced server-side in CQRS handlers via `IOrganizationAuthorizationService`. The React frontend mirrors the same matrix for UI gating.

### Security Rules

- Only **Owner** can delete an organization
- Default organization cannot be deleted
- Only **Admin/Owner** can send invitations
- Only **Owner** can assign the Owner role (ownership transfer)
- **Developer** cannot manage users
- **Viewer** is read-only
- Cannot remove or demote the last Owner

---

## Part 3 — Provider Management

Part 3 introduces a **compute provider abstraction layer**. Organizations can connect external GPU infrastructure providers, validate credentials, inspect available regions/GPUs/templates, and monitor connection health — without coupling business logic to any single vendor API.

### Provider Abstraction

```mermaid
flowchart LR
    subgraph Application
        PS[IProviderService]
        ICP[IComputeProvider]
    end

    subgraph Infrastructure
        Factory[ComputeProviderFactory]
        RunPod[RunPodProvider]
        Future[Vast / Lambda / Azure / AWS / GCP / K8s]
    end

    PS --> Factory
    Factory --> ICP
    ICP --> RunPod
    ICP -.-> Future
```

- **`IComputeProvider`** (Application interface) — vendor-neutral contract: validate credentials, list regions/GPUs/templates, account info, health checks
- **`ComputeProviderFactory`** (Infrastructure) — resolves the correct implementation by `ProviderType`
- **`RunPodProvider`** (Infrastructure) — first concrete implementation using RunPod GraphQL + REST APIs
- **Application layer never imports RunPod-specific code** — new providers are added by implementing `IComputeProvider` and registering in the factory

Supported provider types (enum): `RunPod`, `Vast`, `Lambda`, `Azure`, `AWS`, `GoogleCloud`, `Kubernetes`. Only **RunPod** is implemented in this phase; the enum and factory pattern allow future providers without changing CQRS handlers or controllers.

### Security

- API keys are encrypted at rest via ASP.NET Data Protection (`ProviderCredential`)
- API keys are **never** returned in API responses
- Key rotation is supported via `PUT /providers/{id}` with a new `apiKey`
- Validation and health checks decrypt credentials only in Infrastructure

### Background Health Monitoring

`ProviderHealthWorker` runs every **5 minutes**, checks each enabled provider, stores history in `ProviderHealthHistory`, and updates the current status on `ProviderHealth`.

### Provider Permissions

| Permission | Owner | Admin | Developer | Viewer |
|------------|-------|-------|-----------|--------|
| `Provider.Read` | ✓ | ✓ | ✓ | ✓ |
| `Provider.Create` | ✓ | ✓ | ✓ | |
| `Provider.Update` | ✓ | ✓ | ✓ | |
| `Provider.Delete` | ✓ | ✓ | | |

---

## Part 4 — GPU Pod Management

Part 4 adds full **GPU pod lifecycle management** on top of the provider abstraction. Users can create, view, start, stop, restart, delete, and sync pods — with RunPod as the first `IPodProvider` implementation.

### Pod Provider Abstraction

```mermaid
flowchart LR
    subgraph Application
        PS[IPodService]
        IPP[IPodProvider]
    end

    subgraph Infrastructure
        Factory[PodProviderFactory]
        RunPod[RunPodPodProvider]
        Future[Vast / Lambda / Azure / AWS / GCP / K8s]
    end

    PS --> Factory
    Factory --> IPP
    IPP --> RunPod
    IPP -.-> Future
```

- **`IPodProvider`** — vendor-neutral pod lifecycle contract (create, start, stop, restart, delete, sync)
- **`RunPodPodProvider`** — RunPod REST API implementation (`https://rest.runpod.io/v1/pods`)
- **Application layer never calls RunPod directly** — handlers use `IPodService` only

### Pod Lifecycle

| Status | Description |
|--------|-------------|
| `Creating` | Pod provisioning in progress |
| `Starting` | Pod is starting |
| `Running` | Pod is active and billing |
| `Stopping` | Pod is shutting down |
| `Stopped` | Pod is stopped, volume data preserved |
| `Restarting` | Pod is restarting |
| `Deleting` | Pod termination in progress |
| `Deleted` | Pod removed (soft-deleted in DB) |
| `Failed` | Provisioning or operation failed |

### Real-Time Updates

`PodStatusHub` (SignalR at `/hubs/pods`) broadcasts `PodStatusChanged` events to organization groups whenever pod status changes — from user actions or the `PodStatusSyncWorker` (runs every 60 seconds).

### Pod Permissions

| Permission | Owner | Admin | Developer | Viewer |
|------------|-------|-------|-----------|--------|
| `Pod.Read` | ✓ | ✓ | ✓ | ✓ |
| `Pod.Create` | ✓ | ✓ | ✓ | |
| `Pod.Update` | ✓ | ✓ | ✓ | |
| `Pod.Delete` | ✓ | ✓ | ✓ | |

---

## Architecture

PodPilot follows **Clean Architecture** with **CQRS** (MediatR) separating concerns across layers:

```mermaid
flowchart TB
    subgraph Frontend["Frontend (React + Vite)"]
        WEB[web/]
    end

    subgraph API["PodPilot.Api"]
        CTRL[Controllers]
        MW[Middleware]
        SW[Swagger]
    end

    subgraph Application["PodPilot.Application"]
        CMD[Commands]
        QRY[Queries]
        VAL[Validators]
        BEH[Behaviors]
    end

    subgraph Domain["PodPilot.Domain"]
        ENT[Entities]
        VO[Value Objects]
        EVT[Domain Events]
    end

    subgraph Infrastructure["PodPilot.Infrastructure"]
        EF[EF Core + MySQL]
        ID[ASP.NET Identity]
        JWT[JWT + Refresh Tokens]
        LOG[Serilog]
        CP[Compute Providers]
    end

    subgraph Contracts["PodPilot.Contracts"]
        DTO[API Contracts]
    end

    WEB -->|REST /api/v1| CTRL
    CTRL --> CMD
    CTRL --> QRY
    CMD --> VAL
    CMD --> BEH
    CMD --> ENT
    QRY --> ENT
    CMD --> EF
    QRY --> EF
    EF --> ID
    EF --> JWT
    CTRL --> DTO
```

### Layer Responsibilities

| Layer | Responsibility |
|-------|----------------|
| **Domain** | Business entities, enums, value objects, domain events. No framework dependencies. |
| **Application** | CQRS handlers, FluentValidation, MediatR pipeline behaviors, service interfaces. |
| **Infrastructure** | EF Core persistence, Identity, JWT, Serilog, external service implementations. |
| **Contracts** | API request/response DTOs shared between API and clients. |
| **Api** | HTTP controllers, middleware, Swagger, DI composition root. |

### Authentication Flow

```mermaid
sequenceDiagram
    participant Client
    participant API
    participant Identity
    participant DB

    Client->>API: POST /auth/register
    API->>Identity: Create user + hash password
    API->>DB: Create organization + Owner membership
    API->>DB: Store refresh token
    API-->>Client: JWT (with organization_id) + Refresh Token

    Client->>API: POST /organizations/switch
    API->>DB: Validate membership
    API-->>Client: New JWT with updated organization_id
```

---

## Folder Structure

```
PodPilot/
├── src/
│   ├── PodPilot.Api/              # ASP.NET Core Web API
│   │   ├── Controllers/V1/        # Versioned API controllers
│   │   ├── Middleware/            # Exception, logging, correlation ID
│   │   └── Dockerfile
│   ├── PodPilot.Application/      # CQRS, validators, behaviors
│   ├── PodPilot.Domain/           # Entities, enums, value objects
│   ├── PodPilot.Infrastructure/   # EF Core, Identity, JWT, Serilog
│   └── PodPilot.Contracts/        # API DTOs
├── tests/
│   ├── PodPilot.Application.Tests/
│   └── PodPilot.Api.Tests/
├── web/                           # React + TypeScript + Vite
│   ├── src/
│   │   ├── components/
│   │   ├── pages/
│   │   ├── layouts/
│   │   ├── contexts/
│   │   ├── services/
│   │   ├── hooks/
│   │   ├── types/
│   │   └── utils/
│   └── Dockerfile
├── docker-compose.yml
├── Directory.Build.props
├── .editorconfig
├── stylecop.json
└── README.md
```

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20.19+](https://nodejs.org/) (or 22.x)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for containerized deployment)
- MySQL 8.x running locally on port 3306

---

## Quick Start (Docker Compose)

The recommended way to run PodPilot:

```bash
docker compose up --build
```

| Service | URL |
|---------|-----|
| **Web UI** | http://localhost:3000 |
| **API** | http://localhost:5000 |
| **Swagger** | http://localhost:5000/swagger |
| **Health** | http://localhost:5000/api/v1/health |
| **MySQL** | localhost:3306 (local instance) |

Database migrations run automatically on API startup.

### Docker Services

- **api** — .NET 10 ASP.NET Core API (connects to host MySQL via `host.docker.internal`)
- **web** — React app served via nginx with API proxy

---

## Local Development

### 1. Ensure Local MySQL Is Running

Use your local MySQL instance on port **3306**. Create the database and user if needed:

```sql
CREATE DATABASE IF NOT EXISTS podpilot;
CREATE USER IF NOT EXISTS 'podpilot'@'localhost' IDENTIFIED BY 'podpilot_secret';
GRANT ALL PRIVILEGES ON podpilot.* TO 'podpilot'@'localhost';
FLUSH PRIVILEGES;
```

Update the connection string in `src/PodPilot.Api/appsettings.Development.json` if your credentials differ.

### 2. Run the API

```bash
cd src/PodPilot.Api
dotnet run
```

The API starts at http://localhost:5000 (or the port in `launchSettings.json`). Migrations apply automatically.

### 3. Run the Frontend

```bash
cd web
npm install
npm run dev
```

The frontend starts at http://localhost:5173 with API requests proxied to the backend.

---

## API Endpoints

All endpoints are versioned under `/api/v1/`:

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/auth/register` | No | Register user + organization |
| `POST` | `/auth/login` | No | Authenticate |
| `POST` | `/auth/refresh` | No | Rotate refresh token |
| `POST` | `/auth/logout` | Yes | Revoke refresh token |
| `GET` | `/users/me` | Yes | Current user profile |
| `GET` | `/health` | No | API + database health |

### Organizations (Part 2)

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/organizations` | List user's organizations |
| `GET` | `/organizations/{id}` | Get organization details |
| `POST` | `/organizations` | Create organization |
| `PUT` | `/organizations/{id}` | Update organization |
| `DELETE` | `/organizations/{id}` | Delete organization (Owner only) |
| `POST` | `/organizations/switch` | Switch current organization (re-issues tokens) |
| `GET` | `/organizations/{id}/members` | List members |
| `POST` | `/organizations/{id}/members` | Add existing user as member |
| `DELETE` | `/organizations/{id}/members/{memberId}` | Remove member |
| `PUT` | `/organizations/{id}/members/{memberId}/role` | Update member role |
| `POST` | `/organizations/{id}/invite` | Invite user by email |
| `POST` | `/organizations/accept` | Accept invitation by token |

### Providers (Part 3)

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/providers` | List organization providers |
| `GET` | `/providers/{id}` | Get provider details |
| `POST` | `/providers` | Create provider (validates credentials) |
| `PUT` | `/providers/{id}` | Update provider (supports API key rotation) |
| `DELETE` | `/providers/{id}` | Delete provider |
| `POST` | `/providers/validate` | Validate credentials before creation |
| `POST` | `/providers/{id}/validate` | Re-validate stored provider |
| `GET` | `/providers/{id}/regions` | List available regions |
| `GET` | `/providers/{id}/gpus` | List available GPU types |
| `GET` | `/providers/{id}/templates` | List deployment templates |
| `GET` | `/providers/{id}/health` | Current health + recent history |

### Pods (Part 4)

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/pods` | List organization pods |
| `GET` | `/pods/{id}` | Get pod details |
| `POST` | `/pods` | Create GPU pod |
| `PUT` | `/pods/{id}` | Update pod name/description |
| `DELETE` | `/pods/{id}` | Delete pod (`force` for running pods) |
| `POST` | `/pods/{id}/start` | Start pod |
| `POST` | `/pods/{id}/stop` | Stop pod |
| `POST` | `/pods/{id}/restart` | Restart pod |
| `POST` | `/pods/{id}/sync` | Sync status with provider |

### Example: Register

```bash
curl -X POST http://localhost:5000/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "SecureP@ss1",
    "firstName": "Jane",
    "lastName": "Doe",
    "organizationName": "Acme AI"
  }'
```

---

## Database Schema

| Table | Description |
|-------|-------------|
| `Users` | ASP.NET Identity users (custom `ApplicationUser`) |
| `RefreshTokens` | JWT refresh tokens with rotation support |
| `Organizations` | Multi-tenant organization records |
| `OrganizationMembers` | User-organization memberships with roles |
| `Invitations` | Pending organization invitations |
| `Permissions` | Seeded permission definitions |
| `OrgRoles` | Seeded organization role catalog |
| `RolePermissions` | Role-to-permission mappings |
| `ComputeProviders` | Organization-scoped compute provider configs |
| `ProviderCredentials` | Encrypted API keys (never exposed via API) |
| `ProviderRegions` | Cached/synced region catalog per provider |
| `ProviderGpuTypes` | Cached/synced GPU catalog per provider |
| `ProviderHealthHistory` | Periodic health check history |
| `GpuPods` | Organization-scoped GPU pod records |
| `PodConfigurations` | Deployment configuration per pod |
| `PodEndpoints` | Exposed network endpoints |
| `PodStatusHistory` | Pod status change history |
| `AuditLogs` | Immutable audit trail |
| `Roles` / `UserRoles` | ASP.NET Identity role management |

### Organization Roles

- **Owner** — Full control, can delete org and transfer ownership
- **Admin** — Manage members, invitations, and settings
- **Developer** — Manage workloads, read-only on user management
- **Viewer** — Read-only access

---

## Testing

```bash
# Run all tests
dotnet test

# Application unit tests (validators + permissions)
dotnet test tests/PodPilot.Application.Tests

# API integration tests (auth + organizations + providers + pods)
dotnet test tests/PodPilot.Api.Tests
```

## Frontend (Part 2 + Part 3 + Part 4)

| Page | Route | Description |
|------|-------|-------------|
| Organizations | `/organizations` | List and manage organizations |
| Create Organization | `/organizations/create` | Create new organization |
| Settings | `/organizations/:id/settings` | Edit/delete organization |
| Members | `/members` | Member table, invite, role management |
| Accept Invitation | `/invitations/accept?token=` | Accept email invitation |
| Profile | `/profile` | User profile and memberships |
| Providers | `/providers` | List connected compute providers |
| Add Provider | `/providers/add` | Validate API key, then save |
| Edit Provider | `/providers/:id/edit` | Update provider settings / rotate key |
| Provider Details | `/providers/:id` | Regions, GPUs, templates, health status |
| GPU Pods | `/pods` | Dashboard of running/stopped pods |
| Create Pod | `/pods/create` | Configure and deploy a GPU pod |
| Pod Details | `/pods/:id` | Status, config, endpoints, history |

Key components: `OrganizationSwitcher`, `OrganizationCard`, `MemberTable`, `InvitationModal`, `RoleBadge`, `Avatar`.

---

## Configuration

### JWT Settings (`appsettings.json`)

```json
{
  "Jwt": {
    "Issuer": "PodPilot",
    "Audience": "PodPilot",
    "Secret": "your-256-bit-secret-key-here",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

> **Important:** Change the JWT secret in production. Docker Compose uses environment variable overrides.

### Connection String

```
Server=localhost;Port=3306;Database=podpilot;User=podpilot;Password=podpilot_secret;
```

---

### JWT Settings (`appsettings.json`)

```json
{
  "Jwt": {
    "Issuer": "PodPilot",
    "Audience": "PodPilot",
    "Secret": "your-256-bit-secret-key-here",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

> **Important:** Change the JWT secret in production. Docker Compose uses environment variable overrides.

### Connection String

```
Server=localhost;Port=3306;Database=podpilot;User=podpilot;Password=podpilot_secret;
```

---

## Quality Standards

- **Nullable reference types** enabled solution-wide
- **Treat warnings as errors** enforced via `Directory.Build.props`
- **StyleCop Analyzers** for code style consistency
- **XML documentation** on public APIs (Swagger integration)
- **EditorConfig** for formatting conventions

---

## Logging

Serilog is configured with:

- **Console** output with structured properties
- **Rolling file** logs in `logs/podpilot-*.log` (30-day retention)
- **Request logging** via Serilog middleware
- **Correlation ID** propagated via `X-Correlation-Id` header

---

## What's Next (Part 5+)

Part 4 intentionally excludes:

- Pod auto start / shutdown orchestration engines
- Ollama model management
- AI Gateway / inference providers
- Request queue and model routing

These will be built on top of the organization, provider, and pod layers.

---

## License

Copyright (c) PodPilot. All rights reserved.
