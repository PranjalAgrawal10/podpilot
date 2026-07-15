using PodPilot.Sdk;

var apiUrl = Environment.GetEnvironmentVariable("PODPILOT_API_URL") ?? "http://localhost:5000";
var email = args.ElementAtOrDefault(0) ?? throw new ArgumentException("Usage: Example <email> <password>");
var password = args.ElementAtOrDefault(1) ?? throw new ArgumentException("Usage: Example <email> <password>");

using var client = new PodPilotClient(apiUrl);
var auth = await client.LoginAsync(email, password);
Console.WriteLine($"Logged in. Token type={auth.TokenType}");

var pods = await client.ListPodsAsync();
Console.WriteLine($"Pods: {pods.Count}");
foreach (var pod in pods)
{
    Console.WriteLine($"  {pod.Name} ({pod.Status})");
}

var gateway = await client.GetGatewayStatsAsync();
Console.WriteLine($"Gateway active={gateway.ActiveRequests} errors={gateway.RecentErrors}");
