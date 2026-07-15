# Installation

## Prerequisites

- .NET SDK 10 (`global.json`)
- Node.js 20+ (web)
- MySQL 8.x
- Redis 7.x (gateway rate limits / cache)

## Local API + web

```bash
# Database
# Create schema/user matching ConnectionStrings__DefaultConnection

cp .env.example .env   # optional overrides

dotnet build
dotnet run --project src/PodPilot.Api

cd web && npm install && npm run dev
```

API defaults to `http://localhost:5000` (or Kestrel ports in `launchSettings`). Web Vite app typically uses `http://localhost:5173`.

## Docker Compose

```bash
docker compose up --build
```

- API: `http://localhost:5000`
- Web: `http://localhost:3000`
- Redis: `localhost:6379`

MySQL is **optional** in Compose — see comments in `docker-compose.yml`. By default the API talks to MySQL on the host via `host.docker.internal`.

## Environment variables

| Variable | Description |
|----------|-------------|
| `ConnectionStrings__DefaultConnection` | MySQL |
| `ConnectionStrings__Redis` | Redis |
| `Jwt__Secret` | JWT signing key |
| `Cors__AllowedOrigins__0` | Allowed browser origin |
| `Swagger__Enabled` | Enable Swagger outside Development |
| `PODPILOT_API_URL` | CLI/SDK default API base URL |

## CLI

```bash
dotnet run --project src/PodPilot.Cli -- login --email you@example.com --password '***'
```

See [cli.md](cli.md).
