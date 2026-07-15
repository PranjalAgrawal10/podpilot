# PodPilot Java SDK

Minimal Java 17+ `HttpClient` wrapper (no third-party JSON dependency).

## Build

```bash
cd sdk/java
mvn -q package
```

## Example

```java
import io.podpilot.PodPilotClient;

public class Example {
  public static void main(String[] args) throws Exception {
    String baseUrl = System.getenv().getOrDefault("PODPILOT_API_URL", "http://localhost:5000");
    PodPilotClient client = new PodPilotClient(baseUrl);

    var auth = client.login("you@example.com", "your-password");
    System.out.println("expiresIn=" + auth.expiresIn());

    System.out.println("health=" + client.getHealth().status());

    for (var pod : client.listPods()) {
      System.out.println(pod.id() + " " + pod.name() + " " + pod.status());
    }

    var gw = client.getGatewayStats();
    System.out.println("gateway errors=" + gw.recentErrors() + " avgMs=" + gw.averageLatencyMs());
  }
}
```
