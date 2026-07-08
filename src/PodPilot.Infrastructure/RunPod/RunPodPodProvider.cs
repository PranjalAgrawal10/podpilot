using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Pods;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.RunPod;

/// <summary>
/// RunPod implementation of GPU pod lifecycle operations.
/// </summary>
public sealed class RunPodPodProvider : IPodProvider
{
    private const string GraphQlEndpoint = "https://api.runpod.io/graphql";
    private const string RestBaseUrl = "https://rest.runpod.io/v1";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly IHttpClientFactory httpClientFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="RunPodPodProvider"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    public RunPodPodProvider(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    /// <inheritdoc />
    public ProviderType ProviderType => ProviderType.RunPod;

    /// <inheritdoc />
    public async Task<PodInfo> CreatePodAsync(
        string apiKey,
        PodCreateOptions options,
        CancellationToken cancellationToken = default)
    {
        var payload = new Dictionary<string, object?>
        {
            ["name"] = options.Name,
            ["gpuTypeIds"] = new[] { options.GpuId },
            ["gpuCount"] = options.GpuCount,
            ["containerDiskInGb"] = options.ContainerDiskGb,
            ["volumeInGb"] = options.VolumeDiskGb,
            ["volumeMountPath"] = options.VolumeMountPath,
            ["supportPublicIp"] = options.EnablePublicIp,
        };

        if (!string.IsNullOrWhiteSpace(options.TemplateId))
        {
            payload["templateId"] = options.TemplateId;
        }
        else
        {
            payload["imageName"] = options.ImageName;
        }

        if (!string.IsNullOrWhiteSpace(options.Region))
        {
            payload["dataCenterIds"] = new[] { options.Region };
        }

        if (options.EnvironmentVariables.Count > 0)
        {
            payload["env"] = options.EnvironmentVariables;
        }

        if (options.Ports.Count > 0)
        {
            payload["ports"] = options.Ports;
        }

        using var response = await CreateRestClient(apiKey).PostAsJsonAsync(
            $"{RestBaseUrl}/pods",
            payload,
            JsonOptions,
            cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"RunPod pod creation failed: {content}");
        }

        using var document = JsonDocument.Parse(content);
        return MapPod(document.RootElement);
    }

    /// <inheritdoc />
    public async Task<PodOperationResult> DeletePodAsync(
        string apiKey,
        string providerPodId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await CreateRestClient(apiKey).DeleteAsync(
                $"{RestBaseUrl}/pods/{providerPodId}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return new PodOperationResult
                {
                    Success = false,
                    Status = PodStatus.Failed,
                    ErrorMessage = content,
                };
            }

            return new PodOperationResult
            {
                Success = true,
                Status = PodStatus.Deleted,
            };
        }
        catch (Exception ex)
        {
            return new PodOperationResult
            {
                Success = false,
                Status = PodStatus.Failed,
                ErrorMessage = ex.Message,
            };
        }
    }

    /// <inheritdoc />
    public async Task<PodOperationResult> StartPodAsync(
        string apiKey,
        string providerPodId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var podResponse = await CreateRestClient(apiKey).GetAsync(
                $"{RestBaseUrl}/pods/{providerPodId}",
                cancellationToken);

            var podContent = await podResponse.Content.ReadAsStringAsync(cancellationToken);
            if (!podResponse.IsSuccessStatusCode)
            {
                return CreateFailureResult(podContent);
            }

            using var podDocument = JsonDocument.Parse(podContent);
            var gpuCount = ResolveGpuCount(podDocument.RootElement);

            const string mutation = """
                mutation podResume($input: PodResumeInput!) {
                  podResume(input: $input) {
                    id
                    desiredStatus
                    lastStatusChange
                  }
                }
                """;

            using var graphDocument = await ExecuteGraphQlAsync(
                apiKey,
                mutation,
                new
                {
                    input = new
                    {
                        podId = providerPodId,
                        gpuCount,
                        syncMachine = true,
                    },
                },
                cancellationToken);

            var resumedPodId = graphDocument.RootElement
                .GetProperty("data")
                .GetProperty("podResume")
                .GetProperty("id")
                .GetString() ?? providerPodId;

            var pod = await GetPodAsync(apiKey, resumedPodId, cancellationToken);

            return new PodOperationResult
            {
                Success = true,
                Status = pod.Status == PodStatus.Unknown ? PodStatus.Starting : pod.Status,
                Pod = pod,
            };
        }
        catch (Exception ex)
        {
            return CreateFailureResult(ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task<PodOperationResult> StopPodAsync(
        string apiKey,
        string providerPodId,
        CancellationToken cancellationToken = default) =>
        await ExecuteLifecycleAsync(apiKey, providerPodId, "stop", PodStatus.Stopping, cancellationToken);

    /// <inheritdoc />
    public async Task<PodOperationResult> RestartPodAsync(
        string apiKey,
        string providerPodId,
        CancellationToken cancellationToken = default) =>
        await ExecuteLifecycleAsync(apiKey, providerPodId, "restart", PodStatus.Restarting, cancellationToken);

    /// <inheritdoc />
    public async Task<PodInfo> GetPodAsync(
        string apiKey,
        string providerPodId,
        CancellationToken cancellationToken = default)
    {
        using var response = await CreateRestClient(apiKey).GetAsync(
            $"{RestBaseUrl}/pods/{providerPodId}",
            cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"RunPod get pod failed: {content}");
        }

        using var document = JsonDocument.Parse(content);
        return MapPod(document.RootElement);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PodInfo>> ListPodsAsync(
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        using var response = await CreateRestClient(apiKey).GetAsync(
            $"{RestBaseUrl}/pods",
            cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"RunPod list pods failed: {content}");
        }

        using var document = JsonDocument.Parse(content);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return document.RootElement.EnumerateArray().Select(MapPod).ToList();
    }

    /// <inheritdoc />
    public Task<PodInfo> SyncPodStatusAsync(
        string apiKey,
        string providerPodId,
        CancellationToken cancellationToken = default) =>
        GetPodAsync(apiKey, providerPodId, cancellationToken);

    private async Task<PodOperationResult> ExecuteLifecycleAsync(
        string apiKey,
        string providerPodId,
        string action,
        PodStatus transitionalStatus,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await CreateRestClient(apiKey).PostAsync(
                $"{RestBaseUrl}/pods/{providerPodId}/{action}",
                null,
                cancellationToken);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new PodOperationResult
                {
                    Success = false,
                    Status = PodStatus.Failed,
                    ErrorMessage = content,
                };
            }

            PodInfo? pod = null;
            if (!string.IsNullOrWhiteSpace(content))
            {
                using var document = JsonDocument.Parse(content);
                pod = MapPod(document.RootElement);
            }
            else
            {
                pod = await GetPodAsync(apiKey, providerPodId, cancellationToken);
            }

            return new PodOperationResult
            {
                Success = true,
                Status = pod.Status == PodStatus.Unknown ? transitionalStatus : pod.Status,
                Pod = pod,
            };
        }
        catch (Exception ex)
        {
            return new PodOperationResult
            {
                Success = false,
                Status = PodStatus.Failed,
                ErrorMessage = ex.Message,
            };
        }
    }

    private static PodInfo MapPod(JsonElement element)
    {
        var providerPodId = element.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? string.Empty : string.Empty;
        var name = element.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? string.Empty : string.Empty;
        var desiredStatus = element.TryGetProperty("desiredStatus", out var statusProp)
            ? statusProp.GetString()
            : null;

        var gpuId = element.TryGetProperty("machine", out var machineProp)
            && machineProp.TryGetProperty("gpuTypeId", out var gpuTypeIdProp)
            ? gpuTypeIdProp.GetString()
            : element.TryGetProperty("gpu", out var gpuProp) && gpuProp.TryGetProperty("id", out var gpuIdProp)
                ? gpuIdProp.GetString()
                : null;

        var gpuDisplayName = element.TryGetProperty("machine", out var machineDisplayProp)
            && machineDisplayProp.TryGetProperty("gpuDisplayName", out var gpuDisplayProp)
            ? gpuDisplayProp.GetString()
            : gpuId;

        var region = element.TryGetProperty("machine", out var machineRegionProp)
            && machineRegionProp.TryGetProperty("dataCenterId", out var dataCenterProp)
            ? dataCenterProp.GetString()
            : element.TryGetProperty("machine", out var machineLocationProp)
                && machineLocationProp.TryGetProperty("location", out var locationProp)
                ? locationProp.GetString()
                : null;

        var imageName = element.TryGetProperty("image", out var imageProp) ? imageProp.GetString() : null;
        var templateId = element.TryGetProperty("templateId", out var templateProp) ? templateProp.GetString() : null;
        var publicIp = element.TryGetProperty("publicIp", out var publicIpProp) ? publicIpProp.GetString() : null;
        var containerDisk = element.TryGetProperty("containerDiskInGb", out var containerDiskProp)
            && containerDiskProp.TryGetInt32(out var containerDiskValue)
            ? containerDiskValue
            : (int?)null;
        var volumeDisk = element.TryGetProperty("volumeInGb", out var volumeDiskProp)
            && volumeDiskProp.TryGetInt32(out var volumeDiskValue)
            ? volumeDiskValue
            : (int?)null;
        var hourlyCost = element.TryGetProperty("costPerHr", out var costProp) && costProp.TryGetDecimal(out var costValue)
            ? costValue
            : (decimal?)null;
        var lastStartedAt = element.TryGetProperty("lastStartedAt", out var lastStartedProp)
            && lastStartedProp.ValueKind == JsonValueKind.String
            && DateTime.TryParse(lastStartedProp.GetString(), out var startedAt)
            ? startedAt
            : (DateTime?)null;
        var lastStatusChange = element.TryGetProperty("lastStatusChange", out var lastStatusProp)
            ? lastStatusProp.GetString()
            : null;

        var endpoints = MapEndpoints(element, publicIp);
        var endpoint = endpoints.FirstOrDefault(e => e.Protocol.Equals("http", StringComparison.OrdinalIgnoreCase))?.Url
            ?? endpoints.FirstOrDefault()?.Url;

        return new PodInfo
        {
            ProviderPodId = providerPodId,
            Name = name,
            Status = ResolvePodStatus(desiredStatus, publicIp, lastStatusChange, endpoints, element),
            GpuId = gpuId ?? string.Empty,
            GpuType = MapGpuType(gpuDisplayName ?? gpuId ?? string.Empty),
            Region = region,
            TemplateId = templateId,
            ImageName = imageName,
            ContainerDiskGb = containerDisk,
            VolumeDiskGb = volumeDisk,
            PublicIp = publicIp,
            Endpoint = endpoint,
            Endpoints = endpoints,
            HourlyCost = hourlyCost,
            LastStartedAt = lastStartedAt,
            StatusMessage = lastStatusChange,
        };
    }

    private static IReadOnlyList<PodEndpointInfo> MapEndpoints(JsonElement element, string? publicIp)
    {
        var endpoints = new List<PodEndpointInfo>();

        if (element.TryGetProperty("ports", out var portsProp) && portsProp.ValueKind == JsonValueKind.Array)
        {
            foreach (var portElement in portsProp.EnumerateArray())
            {
                var portValue = portElement.GetString();
                if (string.IsNullOrWhiteSpace(portValue))
                {
                    continue;
                }

                var parts = portValue.Split('/');
                if (parts.Length != 2 || !int.TryParse(parts[0], out var port))
                {
                    continue;
                }

                var protocol = parts[1];
                int? publicPort = null;
                if (element.TryGetProperty("portMappings", out var mappingsProp)
                    && mappingsProp.TryGetProperty(port.ToString(), out var mappedPortProp)
                    && mappedPortProp.TryGetInt32(out var mappedPort))
                {
                    publicPort = mappedPort;
                }

                string? url = null;
                if (!string.IsNullOrWhiteSpace(publicIp) && publicPort.HasValue)
                {
                    url = protocol.Equals("http", StringComparison.OrdinalIgnoreCase)
                        ? $"http://{publicIp}:{publicPort.Value}"
                        : $"{publicIp}:{publicPort.Value}";
                }

                endpoints.Add(new PodEndpointInfo
                {
                    Port = port,
                    Protocol = protocol,
                    PublicPort = publicPort,
                    Url = url,
                });
            }
        }

        return endpoints;
    }

    private static PodStatus MapStatus(string? desiredStatus) =>
        desiredStatus?.ToUpperInvariant() switch
        {
            "RUNNING" => PodStatus.Running,
            "EXITED" => PodStatus.Stopped,
            "TERMINATED" => PodStatus.Deleted,
            "CREATED" => PodStatus.BuildingPending,
            "BUILDING" => PodStatus.BuildingPending,
            "STAGING" => PodStatus.BuildingPending,
            "RESTARTING" => PodStatus.Restarting,
            _ => PodStatus.Unknown,
        };

    private static PodStatus ResolvePodStatus(
        string? desiredStatus,
        string? publicIp,
        string? lastStatusChange,
        IReadOnlyList<PodEndpointInfo> endpoints,
        JsonElement element)
    {
        if (IsBuildingOrStaging(lastStatusChange))
        {
            return PodStatus.BuildingPending;
        }

        var mapped = MapStatus(desiredStatus);
        if (mapped == PodStatus.Running && IsInitializing(publicIp, endpoints, element))
        {
            return PodStatus.BuildingPending;
        }

        return mapped;
    }

    private static bool IsBuildingOrStaging(string? lastStatusChange) =>
        !string.IsNullOrWhiteSpace(lastStatusChange)
        && (lastStatusChange.Contains("building", StringComparison.OrdinalIgnoreCase)
            || lastStatusChange.Contains("staging", StringComparison.OrdinalIgnoreCase)
            || lastStatusChange.Contains("initializ", StringComparison.OrdinalIgnoreCase)
            || lastStatusChange.Contains("pulling image", StringComparison.OrdinalIgnoreCase));

    private static bool IsInitializing(
        string? publicIp,
        IReadOnlyList<PodEndpointInfo> endpoints,
        JsonElement element)
    {
        if (string.IsNullOrWhiteSpace(publicIp))
        {
            return true;
        }

        if (element.TryGetProperty("portMappings", out var mappings)
            && (mappings.ValueKind == JsonValueKind.Null || mappings.ValueKind == JsonValueKind.Undefined))
        {
            return true;
        }

        return endpoints.Count == 0
            || endpoints.All(endpoint => string.IsNullOrWhiteSpace(endpoint.Url));
    }

    private static GpuType MapGpuType(string name)
    {
        var normalized = name.ToUpperInvariant();
        if (normalized.Contains("4090", StringComparison.Ordinal))
        {
            return GpuType.RTX4090;
        }

        if (normalized.Contains("5090", StringComparison.Ordinal))
        {
            return GpuType.RTX5090;
        }

        if (normalized.Contains("A100", StringComparison.Ordinal))
        {
            return GpuType.A100;
        }

        if (normalized.Contains("H100", StringComparison.Ordinal))
        {
            return GpuType.H100;
        }

        if (normalized.Contains("L40S", StringComparison.Ordinal))
        {
            return GpuType.L40S;
        }

        if (normalized.Contains("A40", StringComparison.Ordinal))
        {
            return GpuType.A40;
        }

        if (normalized.Contains("V100", StringComparison.Ordinal))
        {
            return GpuType.V100;
        }

        return GpuType.Custom;
    }

    private static PodOperationResult CreateFailureResult(string? message) =>
        new()
        {
            Success = false,
            Status = PodStatus.Failed,
            ErrorMessage = message,
        };

    private static int ResolveGpuCount(JsonElement element) =>
        element.TryGetProperty("gpuCount", out var gpuCountProp)
        && gpuCountProp.TryGetInt32(out var gpuCount)
        && gpuCount > 0
            ? gpuCount
            : 1;

    private async Task<JsonDocument> ExecuteGraphQlAsync(
        string apiKey,
        string query,
        object variables,
        CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(nameof(RunPodPodProvider));
        var requestUri = $"{GraphQlEndpoint}?api_key={Uri.EscapeDataString(apiKey)}";
        using var response = await client.PostAsJsonAsync(
            requestUri,
            new { query, variables },
            JsonOptions,
            cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"RunPod GraphQL request failed: {content}");
        }

        var document = JsonDocument.Parse(content);
        if (document.RootElement.TryGetProperty("errors", out var errors)
            && errors.ValueKind == JsonValueKind.Array
            && errors.GetArrayLength() > 0)
        {
            var message = errors[0].TryGetProperty("message", out var messageProp)
                ? messageProp.GetString()
                : "RunPod GraphQL request failed.";

            throw new InvalidOperationException(message);
        }

        return document;
    }

    private HttpClient CreateRestClient(string apiKey)
    {
        var client = httpClientFactory.CreateClient(nameof(RunPodPodProvider));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        return client;
    }
}
