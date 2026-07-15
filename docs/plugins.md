# Plugins

Plugins extend PodPilot without modifying core handlers.

## API

| Method | Path |
|--------|------|
| GET | `/api/v1/plugins` |
| GET | `/api/v1/plugins/dashboard` |
| POST | `/api/v1/plugins` |
| GET/PUT | `/api/v1/plugins/{id}` |
| GET/PUT | `/api/v1/plugins/{id}/settings` |
| POST | `/api/v1/plugins/{id}/enable` |
| POST | `/api/v1/plugins/{id}/disable` |

Plugins are organization-scoped. Settings payloads are opaque JSON managed by the plugin definition.

## Guidelines

- Prefer env-backed secrets via the Secrets API rather than embedding keys in plugin settings
- Disable plugins before deleting associated resources
- Audit events are emitted for enable/disable/update
