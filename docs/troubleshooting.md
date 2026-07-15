# Troubleshooting

## API will not start

- Confirm MySQL is reachable with `ConnectionStrings__DefaultConnection`
- Confirm Redis with `ConnectionStrings__Redis`
- Check `src/PodPilot.Api/logs/` for Serilog output

## 401 on API calls

- Login again (`podpilot login` or `POST /api/v1/auth/login`)
- Ensure `Authorization: Bearer <accessToken>`
- If MFA is required, complete MFA in the web UI first

## Gateway 401 / 403

- Use a **gateway API key**, not the user JWT, on `/v1/*`
- Confirm the key is not revoked and rate limits allow traffic

## Pod stuck / model pull fails

- `podpilot status` and `podpilot logs --pod-id <id>`
- Validate provider credentials (`POST /api/v1/providers/{id}/validate`)
- Ensure the pod image includes Ollama (or your inference runtime)

## CORS errors in browser

Add the web origin: `Cors__AllowedOrigins__0=http://localhost:5173`

## Compose healthcheck failing

API image must include `curl`. Wait for `start_period` (40s) after first migrate/boot.
