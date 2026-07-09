using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Models.Observability;
using PodPilot.Application.Observability.Queries.ExportObservability;
using PodPilot.Application.Observability.Queries.GetAnalytics;
using PodPilot.Application.Observability.Queries.GetCost;
using PodPilot.Application.Observability.Queries.GetLiveMetrics;
using PodPilot.Application.Observability.Queries.GetMetrics;
using PodPilot.Application.Observability.Queries.GetPodHealthOverview;
using PodPilot.Application.Observability.Queries.GetProviderHealthOverview;
using PodPilot.Application.Observability.Queries.GetSystemHealth;
using PodPilot.Application.Observability.Queries.ListAlerts;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Observability;
using PodPilot.Domain.Enums;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// Observability and monitoring endpoints.
/// </summary>
[ApiController]
[Authorize]
[Produces("application/json")]
public sealed class ObservabilityController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservabilityController"/> class.
    /// </summary>
    public ObservabilityController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    /// <summary>
    /// Gets historical metrics snapshots.
    /// </summary>
    [HttpGet("api/v1/metrics")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MetricsSnapshotResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetrics(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] Guid? providerId,
        [FromQuery] Guid? podId,
        [FromQuery] string? model,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetMetricsQuery
            {
                From = from,
                To = to,
                ProviderId = providerId,
                PodId = podId,
                ModelName = model,
                Limit = limit,
            },
            cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<MetricsSnapshotResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Gets live dashboard metrics.
    /// </summary>
    [HttpGet("api/v1/metrics/live")]
    [ProducesResponseType(typeof(ApiResponse<LiveMetricsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLiveMetrics(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetLiveMetricsQuery(), cancellationToken);
        return Ok(ApiResponse<LiveMetricsResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Gets cost summary.
    /// </summary>
    [HttpGet("api/v1/cost")]
    [ProducesResponseType(typeof(ApiResponse<CostResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCost(
        [FromQuery] string? period,
        [FromQuery] Guid? providerId,
        [FromQuery] Guid? podId,
        [FromQuery] string? model,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetCostQuery
            {
                Period = ParseMetricsPeriod(period),
                ProviderId = providerId,
                PodId = podId,
                ModelName = model,
            },
            cancellationToken);

        return Ok(ApiResponse<CostResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Gets usage analytics.
    /// </summary>
    [HttpGet("api/v1/analytics")]
    [ProducesResponseType(typeof(ApiResponse<AnalyticsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnalytics(
        [FromQuery] string? period,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] Guid? providerId,
        [FromQuery] Guid? podId,
        [FromQuery] string? model,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetAnalyticsQuery
            {
                Period = ParseMetricsPeriod(period),
                From = from,
                To = to,
                ProviderId = providerId,
                PodId = podId,
                ModelName = model,
            },
            cancellationToken);

        return Ok(ApiResponse<AnalyticsResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Gets system health status.
    /// </summary>
    [HttpGet("api/v1/health/system")]
    [ProducesResponseType(typeof(ApiResponse<SystemHealthResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSystemHealth(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSystemHealthQuery(), cancellationToken);
        return Ok(ApiResponse<SystemHealthResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Gets pod health overview.
    /// </summary>
    [HttpGet("api/v1/health/pods")]
    [ProducesResponseType(typeof(ApiResponse<PodHealthOverviewResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPodHealth(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPodHealthOverviewQuery(), cancellationToken);
        return Ok(ApiResponse<PodHealthOverviewResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Gets provider health overview.
    /// </summary>
    [HttpGet("api/v1/health/providers")]
    [ProducesResponseType(typeof(ApiResponse<ProviderHealthOverviewResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProviderHealth(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProviderHealthOverviewQuery(), cancellationToken);
        return Ok(ApiResponse<ProviderHealthOverviewResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Lists alerts.
    /// </summary>
    [HttpGet("api/v1/alerts")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AlertResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAlerts(
        [FromQuery] bool activeOnly = false,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new ListAlertsQuery { ActiveOnly = activeOnly, Limit = limit },
            cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<AlertResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Exports observability data.
    /// </summary>
    [HttpGet("api/v1/observability/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Export(
        [FromQuery] string? format,
        [FromQuery] string? type,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] Guid? providerId,
        [FromQuery] Guid? podId,
        [FromQuery] string? model,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new ExportObservabilityQuery
            {
                Format = ParseExportFormat(format),
                ExportType = ParseExportType(type),
                From = from,
                To = to,
                ProviderId = providerId,
                PodId = podId,
                ModelName = model,
            },
            cancellationToken);

        return File(result.Content, result.ContentType, result.FileName);
    }

    private string? GetCorrelationId() =>
        HttpContext.Items["CorrelationId"]?.ToString();

    private static MetricsPeriod ParseMetricsPeriod(string? value) =>
        Enum.TryParse<MetricsPeriod>(value, ignoreCase: true, out var period)
            ? period
            : MetricsPeriod.Hourly;

    private static ExportFormat ParseExportFormat(string? value) =>
        Enum.TryParse<ExportFormat>(value, ignoreCase: true, out var format)
            ? format
            : ExportFormat.Csv;

    private static ObservabilityExportType ParseExportType(string? value) =>
        Enum.TryParse<ObservabilityExportType>(value, ignoreCase: true, out var exportType)
            ? exportType
            : ObservabilityExportType.Metrics;
}
