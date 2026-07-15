from __future__ import annotations

import json
import os
from dataclasses import dataclass
from typing import Any, Optional
from urllib.error import HTTPError, URLError
from urllib.request import Request, urlopen


class PodPilotError(Exception):
    """Raised when the PodPilot API returns an error."""


@dataclass
class PodSummary:
    id: str
    name: str
    status: str


@dataclass
class HealthStatus:
    status: str
    total_duration: str | None = None


@dataclass
class GatewayHealth:
    active_requests: int
    recent_errors: int
    average_latency_ms: float


class PodPilotClient:
    """Minimal urllib-based client for PodPilot."""

    def __init__(self, base_url: Optional[str] = None, access_token: Optional[str] = None) -> None:
        env = os.environ.get("PODPILOT_API_URL", "http://localhost:5000")
        self.base_url = (base_url or env).rstrip("/")
        self.access_token = access_token

    def login(self, email: str, password: str) -> dict[str, Any]:
        data = self._request("POST", "/api/v1/auth/login", {"email": email, "password": password}, auth=False)
        self.access_token = data["accessToken"]
        return data

    def list_pods(self) -> list[PodSummary]:
        items = self._request("GET", "/api/v1/pods")
        return [PodSummary(id=p["id"], name=p["name"], status=p["status"]) for p in items]

    def get_health(self) -> HealthStatus:
        data = self._request("GET", "/api/v1/health", auth=False)
        return HealthStatus(status=data["status"], total_duration=str(data.get("totalDuration")))

    def get_gateway_stats(self) -> GatewayHealth:
        data = self._request("GET", "/api/v1/gateway/stats")
        return GatewayHealth(
            active_requests=int(data.get("activeRequests", 0)),
            recent_errors=int(data.get("recentErrors", 0)),
            average_latency_ms=float(data.get("averageLatencyMs", 0)),
        )

    def _request(
        self,
        method: str,
        path: str,
        body: Optional[dict[str, Any]] = None,
        auth: bool = True,
    ) -> Any:
        headers = {"Accept": "application/json"}
        payload: bytes | None = None
        if body is not None:
            headers["Content-Type"] = "application/json"
            payload = json.dumps(body).encode("utf-8")
        if auth:
            if not self.access_token:
                raise PodPilotError("Not authenticated. Call login() or pass access_token.")
            headers["Authorization"] = f"Bearer {self.access_token}"

        request = Request(f"{self.base_url}{path}", data=payload, headers=headers, method=method)
        try:
            with urlopen(request, timeout=60) as response:
                raw = response.read().decode("utf-8")
        except HTTPError as exc:
            raw = exc.read().decode("utf-8", errors="replace")
            raise PodPilotError(f"HTTP {exc.code}: {raw[:300]}") from exc
        except URLError as exc:
            raise PodPilotError(f"Network error: {exc.reason}") from exc

        envelope = json.loads(raw)
        if not envelope.get("success") or "data" not in envelope:
            raise PodPilotError(envelope.get("message") or f"Unexpected response: {raw[:300]}")
        return envelope["data"]
