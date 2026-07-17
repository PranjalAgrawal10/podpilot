using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Deployments;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Deployments.Runtimes;

namespace PodPilot.Application.Tests.Deployments;

public class OllamaRuntimeValidationTests
{
    [Fact]
    public async Task Validate_Rejects_Non_Cuda_12()
    {
        var provider = CreateProvider();

        await Assert.ThrowsAsync<ValidationException>(() =>
            provider.ValidateAsync(
                new RuntimeValidationContext
                {
                    Runtime = InferenceRuntimeKind.Ollama,
                    CudaVersion = "11.8",
                    GpuVramGb = 24,
                    RequiredVramGb = 8,
                    CudaCapability = "8.9",
                }));
    }

    [Fact]
    public async Task Validate_Rejects_Insufficient_Vram()
    {
        var provider = CreateProvider();

        await Assert.ThrowsAsync<ValidationException>(() =>
            provider.ValidateAsync(
                new RuntimeValidationContext
                {
                    Runtime = InferenceRuntimeKind.Ollama,
                    CudaVersion = "12.4",
                    GpuVramGb = 8,
                    RequiredVramGb = 24,
                    CudaCapability = "8.9",
                }));
    }

    [Fact]
    public async Task Validate_Accepts_Cuda12_With_Enough_Vram()
    {
        var provider = CreateProvider();

        await provider.ValidateAsync(
            new RuntimeValidationContext
            {
                Runtime = InferenceRuntimeKind.Ollama,
                CudaVersion = "12.4",
                GpuVramGb = 32,
                RequiredVramGb = 24,
                CudaCapability = "8.9",
            });
    }

    private static OllamaRuntimeProvider CreateProvider() =>
        new(
            new StubHttpClientFactory(),
            new Mock<IOllamaClient>().Object,
            NullLogger<OllamaRuntimeProvider>.Instance);

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }
}
