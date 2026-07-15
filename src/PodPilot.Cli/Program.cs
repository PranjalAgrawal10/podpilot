using System.CommandLine;
using PodPilot.Cli;
using PodPilot.Contracts.Pods;

var apiUrlOption = new Option<string>("--api-url")
{
    Description = "PodPilot API base URL (overrides PODPILOT_API_URL)",
};

var root = new RootCommand("PodPilot CLI — manage providers, pods, models, and the AI gateway");
root.Options.Add(apiUrlOption);

root.Subcommands.Add(BuildLoginCommand(apiUrlOption));
root.Subcommands.Add(BuildProviderAddCommand(apiUrlOption));
root.Subcommands.Add(BuildPodCreateCommand(apiUrlOption));
root.Subcommands.Add(BuildModelPullCommand(apiUrlOption));
root.Subcommands.Add(BuildGatewayTestCommand(apiUrlOption));
root.Subcommands.Add(BuildStatusCommand(apiUrlOption));
root.Subcommands.Add(BuildLogsCommand(apiUrlOption));

try
{
    return await root.Parse(args).InvokeAsync();
}
catch (CliException ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}
catch (HttpRequestException ex)
{
    Console.Error.WriteLine($"Network error: {ex.Message}");
    Console.Error.WriteLine("Check PODPILOT_API_URL and that the API is reachable.");
    return 1;
}

static Command BuildLoginCommand(Option<string> apiUrlOption)
{
    var emailOption = new Option<string>("--email") { Required = true, Description = "Account email" };
    var passwordOption = new Option<string>("--password") { Required = true, Description = "Account password" };
    var cmd = new Command("login", "Authenticate and save credentials to ~/.podpilot/credentials.json");
    cmd.Options.Add(emailOption);
    cmd.Options.Add(passwordOption);

    cmd.SetAction(async (parseResult, cancellationToken) =>
    {
        var apiUrl = ResolveApiUrl(parseResult, apiUrlOption);
        var email = parseResult.GetValue(emailOption)!;
        var password = parseResult.GetValue(passwordOption)!;
        var store = new CredentialStore();

        using var client = new ApiClient(apiUrl);
        var auth = await client.LoginAsync(email, password, cancellationToken);

        if (auth.RequiresMfa)
        {
            Console.Error.WriteLine("MFA is required for this account. Complete MFA in the web UI, then try again.");
            return 1;
        }

        await store.SaveAsync(
            new StoredCredentials
            {
                AccessToken = auth.AccessToken,
                RefreshToken = auth.RefreshToken,
                Email = email,
                ApiUrl = apiUrl,
                SavedAt = DateTimeOffset.UtcNow,
            },
            cancellationToken);

        Console.WriteLine($"Logged in as {auth.User.Email}");
        Console.WriteLine($"Organization: {auth.User.CurrentOrganizationId}");
        Console.WriteLine($"Credentials saved to {store.CredentialsFilePath}");
        return 0;
    });

    return cmd;
}

static Command BuildProviderAddCommand(Option<string> apiUrlOption)
{
    var nameOption = new Option<string>("--name") { Required = true };
    var typeOption = new Option<string>("--type") { Required = true, Description = "Provider type (e.g. RunPod)" };
    var displayOption = new Option<string>("--display-name") { Required = true };
    var apiKeyOption = new Option<string>("--api-key") { Required = true };
    var regionOption = new Option<string?>("--region") { Description = "Default region" };

    var cmd = new Command("provider") { Description = "Manage compute providers" };
    var add = new Command("add", "Register a compute provider");
    add.Options.Add(nameOption);
    add.Options.Add(typeOption);
    add.Options.Add(displayOption);
    add.Options.Add(apiKeyOption);
    add.Options.Add(regionOption);

    add.SetAction(async (parseResult, cancellationToken) =>
    {
        using var client = await CreateAuthenticatedClientAsync(parseResult, apiUrlOption, cancellationToken);
        var provider = await client.AddProviderAsync(
            parseResult.GetValue(nameOption)!,
            parseResult.GetValue(typeOption)!,
            parseResult.GetValue(displayOption)!,
            parseResult.GetValue(apiKeyOption)!,
            parseResult.GetValue(regionOption),
            cancellationToken);

        Console.WriteLine($"Provider created: {provider.Id}");
        Console.WriteLine($"  Name: {provider.DisplayName} ({provider.ProviderType})");
        return 0;
    });

    cmd.Subcommands.Add(add);
    return cmd;
}

static Command BuildPodCreateCommand(Option<string> apiUrlOption)
{
    var providerIdOption = new Option<Guid>("--provider-id") { Required = true };
    var nameOption = new Option<string>("--name") { Required = true };
    var gpuIdOption = new Option<string>("--gpu-id") { Required = true };
    var gpuTypeOption = new Option<string>("--gpu-type") { Required = true };
    var regionOption = new Option<string>("--region") { Required = true };
    var imageOption = new Option<string>("--image") { Required = true, Description = "Container image" };
    var gpuCountOption = new Option<int>("--gpu-count") { DefaultValueFactory = _ => 1 };
    var diskOption = new Option<int>("--disk-gb") { DefaultValueFactory = _ => 50 };
    var volumeOption = new Option<int>("--volume-gb") { DefaultValueFactory = _ => 20 };

    var cmd = new Command("pod") { Description = "Manage GPU pods" };
    var create = new Command("create", "Create a GPU pod");
    create.Options.Add(providerIdOption);
    create.Options.Add(nameOption);
    create.Options.Add(gpuIdOption);
    create.Options.Add(gpuTypeOption);
    create.Options.Add(regionOption);
    create.Options.Add(imageOption);
    create.Options.Add(gpuCountOption);
    create.Options.Add(diskOption);
    create.Options.Add(volumeOption);

    create.SetAction(async (parseResult, cancellationToken) =>
    {
        using var client = await CreateAuthenticatedClientAsync(parseResult, apiUrlOption, cancellationToken);
        var body = new CreatePodRequest
        {
            ProviderId = parseResult.GetValue(providerIdOption),
            Name = parseResult.GetValue(nameOption)!,
            GpuId = parseResult.GetValue(gpuIdOption)!,
            GpuType = parseResult.GetValue(gpuTypeOption)!,
            Region = parseResult.GetValue(regionOption)!,
            ImageName = parseResult.GetValue(imageOption)!,
            GpuCount = parseResult.GetValue(gpuCountOption),
            ContainerDiskGb = parseResult.GetValue(diskOption),
            VolumeDiskGb = parseResult.GetValue(volumeOption),
        };

        var pod = await client.CreatePodAsync(body, cancellationToken);
        Console.WriteLine($"Pod created: {pod.Id}");
        Console.WriteLine($"  Name: {pod.Name}");
        Console.WriteLine($"  Status: {pod.Status}");
        return 0;
    });

    cmd.Subcommands.Add(create);
    return cmd;
}

static Command BuildModelPullCommand(Option<string> apiUrlOption)
{
    var podIdOption = new Option<Guid>("--pod-id") { Required = true };
    var modelOption = new Option<string>("--model") { Required = true, Description = "Model ref (e.g. llama3:latest)" };

    var cmd = new Command("model") { Description = "Manage Ollama models" };
    var pull = new Command("pull", "Pull a model onto a pod");
    pull.Options.Add(podIdOption);
    pull.Options.Add(modelOption);

    pull.SetAction(async (parseResult, cancellationToken) =>
    {
        using var client = await CreateAuthenticatedClientAsync(parseResult, apiUrlOption, cancellationToken);
        var result = await client.PullModelAsync(
            parseResult.GetValue(podIdOption),
            parseResult.GetValue(modelOption)!,
            cancellationToken);

        Console.WriteLine($"Pull started: {result.Id}");
        Console.WriteLine($"  Model: {result.ModelName}");
        Console.WriteLine($"  Status: {result.Status} ({result.Progress}%)");
        return 0;
    });

    cmd.Subcommands.Add(pull);
    return cmd;
}

static Command BuildGatewayTestCommand(Option<string> apiUrlOption)
{
    var cmd = new Command("gateway") { Description = "AI gateway commands" };
    var test = new Command("test", "Verify API health and gateway stats");

    test.SetAction(async (parseResult, cancellationToken) =>
    {
        using var client = await CreateAuthenticatedClientAsync(parseResult, apiUrlOption, cancellationToken);
        var health = await client.GetHealthAsync(cancellationToken);
        Console.WriteLine($"Health: {health.Status} ({health.TotalDuration.TotalMilliseconds:F0} ms)");
        foreach (var check in health.Checks)
        {
            Console.WriteLine($"  - {check.Key}: {check.Value.Status}");
        }

        var stats = await client.GetGatewayStatsAsync(cancellationToken);
        Console.WriteLine("Gateway stats:");
        Console.WriteLine($"  Active: {stats.ActiveRequests}");
        Console.WriteLine($"  Streaming: {stats.StreamingRequests}");
        Console.WriteLine($"  Waiting pods: {stats.WaitingPods}");
        Console.WriteLine($"  Avg latency: {stats.AverageLatencyMs:F1} ms");
        Console.WriteLine($"  Recent errors: {stats.RecentErrors}");
        return 0;
    });

    cmd.Subcommands.Add(test);
    return cmd;
}

static Command BuildStatusCommand(Option<string> apiUrlOption)
{
    var cmd = new Command("status", "Show API health, pods, and gateway summary");

    cmd.SetAction(async (parseResult, cancellationToken) =>
    {
        using var client = await CreateAuthenticatedClientAsync(parseResult, apiUrlOption, cancellationToken);
        var health = await client.GetHealthAsync(cancellationToken);
        Console.WriteLine($"API: {client.BaseUrl}");
        Console.WriteLine($"Health: {health.Status}");

        var pods = await client.ListPodsAsync(cancellationToken);
        Console.WriteLine($"Pods ({pods.Count}):");
        if (pods.Count == 0)
        {
            Console.WriteLine("  (none)");
        }
        else
        {
            foreach (var pod in pods)
            {
                Console.WriteLine($"  {pod.Id}  {pod.Name,-24}  {pod.Status}");
            }
        }

        var stats = await client.GetGatewayStatsAsync(cancellationToken);
        Console.WriteLine(
            $"Gateway: active={stats.ActiveRequests} errors={stats.RecentErrors} avgLatency={stats.AverageLatencyMs:F1}ms");
        return 0;
    });

    return cmd;
}

static Command BuildLogsCommand(Option<string> apiUrlOption)
{
    var podIdOption = new Option<Guid?>("--pod-id") { Description = "Pod activity logs (omit for gateway request log)" };
    var limitOption = new Option<int>("--limit") { DefaultValueFactory = _ => 25 };

    var cmd = new Command("logs", "Show pod activity or recent gateway requests");
    cmd.Options.Add(podIdOption);
    cmd.Options.Add(limitOption);

    cmd.SetAction(async (parseResult, cancellationToken) =>
    {
        using var client = await CreateAuthenticatedClientAsync(parseResult, apiUrlOption, cancellationToken);
        var podId = parseResult.GetValue(podIdOption);
        var limit = parseResult.GetValue(limitOption);

        if (podId is Guid id)
        {
            var activity = await client.GetPodActivityAsync(id, cancellationToken);
            Console.WriteLine($"Pod activity for {id} ({activity.Count} events):");
            foreach (var item in activity.Take(limit))
            {
                Console.WriteLine($"  {item.Timestamp:u}  {item.ActivityType,-16}  {item.Source}  {item.Metadata}");
            }
        }
        else
        {
            var requests = await client.ListGatewayRequestsAsync(limit, cancellationToken);
            Console.WriteLine($"Gateway requests (last {requests.Count}):");
            foreach (var req in requests)
            {
                Console.WriteLine(
                    $"  {req.StartedAt:u}  {req.Path}  status={req.Status}  {req.TotalLatencyMs}ms  model={req.Model}");
            }
        }

        return 0;
    });

    return cmd;
}

static string ResolveApiUrl(ParseResult parseResult, Option<string> apiUrlOption)
{
    var fromFlag = parseResult.GetValue(apiUrlOption);
    if (!string.IsNullOrWhiteSpace(fromFlag))
    {
        return fromFlag.TrimEnd('/');
    }

    var fromEnv = Environment.GetEnvironmentVariable("PODPILOT_API_URL");
    if (!string.IsNullOrWhiteSpace(fromEnv))
    {
        return fromEnv.TrimEnd('/');
    }

    return "http://localhost:5000";
}

static async Task<ApiClient> CreateAuthenticatedClientAsync(
    ParseResult parseResult,
    Option<string> apiUrlOption,
    CancellationToken cancellationToken)
{
    var store = new CredentialStore();
    var creds = await store.LoadAsync(cancellationToken);
    if (creds is null || string.IsNullOrWhiteSpace(creds.AccessToken))
    {
        throw new CliException("Not logged in. Run: podpilot login --email you@example.com --password ****");
    }

    var apiUrl = ResolveApiUrl(parseResult, apiUrlOption);
    if (string.IsNullOrWhiteSpace(parseResult.GetValue(apiUrlOption))
        && !string.IsNullOrWhiteSpace(creds.ApiUrl))
    {
        apiUrl = creds.ApiUrl.TrimEnd('/');
    }

    return new ApiClient(apiUrl, creds.AccessToken);
}
