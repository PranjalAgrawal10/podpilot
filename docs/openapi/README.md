# OpenAPI / Swagger

When Swagger is enabled, the interactive UI is available at:

**`/swagger`**

Enable with:

```bash
Swagger__Enabled=true
```

In Development this is typically on by default; in Production/Compose set the env var explicitly (Compose sets `Swagger__Enabled=true` for local docker).

## Tips

- Authenticate via the Swagger “Authorize” button with `Bearer <jwt>`
- Gateway `/v1/*` routes use the gateway authentication scheme — test them with a gateway API key header as documented in the API
- Prefer downloading the OpenAPI document from Swagger UI for codegen rather than hand-maintaining a duplicate spec here
