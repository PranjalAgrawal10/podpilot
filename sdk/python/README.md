# PodPilot Python SDK

## Install

```bash
cd sdk/python
pip install -e .
```

## Example

```python
import os
from podpilot import PodPilotClient

client = PodPilotClient(base_url=os.environ.get("PODPILOT_API_URL", "http://localhost:5000"))
auth = client.login("you@example.com", "your-password")
print("expires_in", auth["expiresIn"])

print("health", client.get_health().status)

for pod in client.list_pods():
    print(pod.id, pod.name, pod.status)

gateway = client.get_gateway_stats()
print("gateway errors", gateway.recent_errors, "avg ms", gateway.average_latency_ms)
```
