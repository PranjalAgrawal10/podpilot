# MCP (Model Context Protocol)

PodPilot can register MCP servers and expose their tools/resources to gateway consumers.

## API

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/v1/mcp/kinds` | Supported server kinds |
| GET/POST | `/api/v1/mcp/servers` | List / register |
| DELETE | `/api/v1/mcp/servers/{id}` | Remove |
| GET | `/api/v1/mcp/tools` | Discovered tools |
| GET | `/api/v1/mcp/resources` | Discovered resources |
| POST | `/api/v1/mcp/tools/execute` | Execute a tool |

## Security

- Connection secrets should use org Secrets — not plaintext in server config where avoidable
- Tool execution is permission-gated and audited
- Prefer least-privilege credentials on remote MCP servers

## IDE usage

Point agent tools at PodPilot’s gateway (`/v1/...`) while using MCP servers registered in PodPilot for org-scoped tooling. See [samples/](../samples/).
