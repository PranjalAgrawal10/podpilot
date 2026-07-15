using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Plugins;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Plugins;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// Plugin catalog and lifecycle endpoints.
/// </summary>
[ApiController]
[Authorize]
[Produces("application/json")]
public sealed class PluginsController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginsController"/> class.
    /// </summary>
    public PluginsController(IMediator mediator) => this.mediator = mediator;

    /// <summary>Lists plugins (local marketplace + installations).</summary>
    [HttpGet("api/v1/plugins")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PluginResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListPluginsQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PluginResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Gets plugin dashboard metrics.</summary>
    [HttpGet("api/v1/plugins/dashboard")]
    [ProducesResponseType(typeof(ApiResponse<PluginDashboardResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPluginDashboardQuery(), cancellationToken);
        return Ok(ApiResponse<PluginDashboardResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Gets an installed plugin.</summary>
    [HttpGet("api/v1/plugins/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PluginResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPluginQuery { InstallationId = id }, cancellationToken);
        return Ok(ApiResponse<PluginResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Installs a plugin.</summary>
    [HttpPost("api/v1/plugins")]
    [ProducesResponseType(typeof(ApiResponse<PluginResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Install([FromBody] InstallPluginRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new InstallPluginCommand
            {
                PackageId = request.PackageId,
                GrantedPermissions = request.GrantedPermissions ?? [],
            },
            cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.InstallationId }, ApiResponse<PluginResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Updates plugin permissions.</summary>
    [HttpPut("api/v1/plugins/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PluginResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePluginRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdatePluginCommand
            {
                InstallationId = id,
                GrantedPermissions = request.GrantedPermissions ?? [],
            },
            cancellationToken);
        return Ok(ApiResponse<PluginResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Uninstalls a plugin.</summary>
    [HttpDelete("api/v1/plugins/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new UninstallPluginCommand { InstallationId = id }, cancellationToken);
        return NoContent();
    }

    /// <summary>Gets plugin settings.</summary>
    [HttpGet("api/v1/plugins/{id:guid}/settings")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PluginSettingResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSettings(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPluginSettingsQuery { InstallationId = id }, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PluginSettingResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Updates plugin settings.</summary>
    [HttpPut("api/v1/plugins/{id:guid}/settings")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateSettings(
        Guid id,
        [FromBody] UpdatePluginSettingsRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new UpdatePluginSettingsCommand
            {
                InstallationId = id,
                Settings = request.Settings,
                SecretKeys = new HashSet<string>(request.SecretKeys ?? [], StringComparer.OrdinalIgnoreCase),
            },
            cancellationToken);
        return NoContent();
    }

    /// <summary>Enables a plugin.</summary>
    [HttpPost("api/v1/plugins/{id:guid}/enable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Enable(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new EnablePluginCommand { InstallationId = id }, cancellationToken);
        return NoContent();
    }

    /// <summary>Disables a plugin.</summary>
    [HttpPost("api/v1/plugins/{id:guid}/disable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Disable(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DisablePluginCommand { InstallationId = id }, cancellationToken);
        return NoContent();
    }

    private string? GetCorrelationId() =>
        HttpContext.Items.TryGetValue("CorrelationId", out var value) ? value?.ToString() : HttpContext.TraceIdentifier;
}
