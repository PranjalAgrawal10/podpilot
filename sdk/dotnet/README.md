# PodPilot .NET SDK

Typed `HttpClient` wrapper for PodPilot (`login`, `list pods`, gateway health).

## Install

```bash
dotnet add reference path/to/sdk/dotnet/PodPilot.Sdk/PodPilot.Sdk.csproj
```

## Example

```csharp
using PodPilot.Sdk;

var apiUrl = Environment.GetEnvironmentVariable("PODPILOT_API_URL") ?? "http://localhost:5000";

using var client = new PodPilotClient(apiUrl);

var auth = await client.LoginAsync("you@example.com", "your-password");
Console.WriteLine($"Token acquired (expires in {auth.ExpiresIn}s)");

var health = await client.GetHealthAsync();
Console.WriteLine($"API health: {health.Status}");

var pods = await client.ListPodsAsync();
foreach (var pod in pods)
{
    Console.WriteLine($"{pod.Id}  {pod.Name}  {pod.Status}");
}

var gateway = await client.GetGatewayStatsAsync();
Console.WriteLine($"Gateway errors (1h): {gateway.RecentErrors}, avg latency {gateway.AverageLatencyMs:F1}ms");
```

Set `PODPILOT_API_URL` when pointing at a non-local deployment.
