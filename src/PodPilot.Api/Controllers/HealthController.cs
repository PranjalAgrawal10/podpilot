using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Health;

namespace PodPilot.Api.Controllers;

/// <summary>
/// Health check endpoints.
/// </summary>
[ApiController]
[Route("api/v1")]
[Produces("application/json")]
public sealed class HealthController : ControllerBase
{
    private readonly HealthCheckService healthCheckService;

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthController"/> class.
    /// </summary>
    /// <param name="healthCheckService">The health check service.</param>
    public HealthController(HealthCheckService healthCheckService)
    {
        this.healthCheckService = healthCheckService;
    }

    /// <summary>
    /// Returns the application health status.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Health check results.</returns>
    [HttpGet("health")]
    [ProducesResponseType(typeof(ApiResponse<HealthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HealthResponse>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetHealth(CancellationToken cancellationToken)
    {
        var report = await healthCheckService.CheckHealthAsync(cancellationToken);

        var response = new HealthResponse
        {
            Status = report.Status.ToString(),
            TotalDuration = report.TotalDuration,
            Checks = report.Entries.ToDictionary(
                e => e.Key,
                e => new HealthCheckEntry
                {
                    Status = e.Value.Status.ToString(),
                    Description = e.Value.Description,
                    Duration = e.Value.Duration,
                }),
        };

        var apiResponse = ApiResponse<HealthResponse>.Ok(response, GetCorrelationId());

        return report.Status == HealthStatus.Healthy
            ? Ok(apiResponse)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, apiResponse);
    }

    private string? GetCorrelationId() =>
        HttpContext.Items["CorrelationId"]?.ToString();
}
