using System.Text.Json;

namespace PodPilot.Cli;

internal sealed class CredentialStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    private readonly string credentialsPath;

    public CredentialStore()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var dir = Path.Combine(home, ".podpilot");
        Directory.CreateDirectory(dir);
        this.credentialsPath = Path.Combine(dir, "credentials.json");
    }

    public string CredentialsFilePath => this.credentialsPath;

    public async Task SaveAsync(StoredCredentials credentials, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(credentials, JsonOptions);
        await File.WriteAllTextAsync(this.credentialsPath, json, cancellationToken);
    }

    public async Task<StoredCredentials?> LoadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(this.credentialsPath))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(this.credentialsPath, cancellationToken);
        return JsonSerializer.Deserialize<StoredCredentials>(json, JsonOptions);
    }
}

internal sealed class StoredCredentials
{
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string ApiUrl { get; set; } = string.Empty;

    public DateTimeOffset SavedAt { get; set; }
}
