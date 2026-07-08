using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Ollama;

namespace PodPilot.Application.Tests.Ollama;

public class OllamaUrlHelperTests
{
    [Fact]
    public void GetOllamaBaseUrl_UsesMappedOllamaEndpoint_WhenPrimaryEndpointIsDifferentPort()
    {
        var pod = new GpuPod
        {
            Name = "training-pod",
            Endpoint = "http://100.65.0.119:8888",
            PublicIp = "100.65.0.119",
            Endpoints =
            [
                new PodEndpoint
                {
                    Port = 8888,
                    Protocol = "http",
                    PublicPort = 8888,
                    Url = "http://100.65.0.119:8888",
                },
                new PodEndpoint
                {
                    Port = 11434,
                    Protocol = "http",
                    PublicPort = 31434,
                    Url = "http://100.65.0.119:31434",
                },
            ],
        };

        var baseUrl = OllamaUrlHelper.GetOllamaBaseUrl(pod);

        Assert.Equal("http://100.65.0.119:31434", baseUrl);
    }

    [Fact]
    public void GetOllamaBaseUrl_UsesPublicIpAndOllamaPort_WhenOnlyPrimaryEndpointExists()
    {
        var pod = new GpuPod
        {
            Name = "legacy-pod",
            Endpoint = "http://100.65.0.119:8888",
            PublicIp = "100.65.0.119",
            Endpoints =
            [
                new PodEndpoint
                {
                    Port = 8888,
                    Protocol = "http",
                    PublicPort = 8888,
                    Url = "http://100.65.0.119:8888",
                },
            ],
        };

        var baseUrl = OllamaUrlHelper.GetOllamaBaseUrl(pod);

        Assert.Equal("http://100.65.0.119:11434", baseUrl);
    }

    [Fact]
    public void GetOllamaBaseUrl_UsesEndpoint_WhenItAlreadyTargetsOllamaPort()
    {
        var pod = new GpuPod
        {
            Name = "ollama-pod",
            Endpoint = "http://127.0.0.1:11434",
            Endpoints = [],
        };

        var baseUrl = OllamaUrlHelper.GetOllamaBaseUrl(pod);

        Assert.Equal("http://127.0.0.1:11434", baseUrl);
    }
}
