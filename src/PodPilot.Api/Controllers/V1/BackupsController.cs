using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Commercial;
using PodPilot.Contracts.Commercial;
using PodPilot.Contracts.Common;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// Backup and restore endpoints.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/backups")]
[Produces("application/json")]
public sealed class BackupsController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackupsController"/> class.
    /// </summary>
    public BackupsController(IMediator mediator) => this.mediator = mediator;

    /// <summary>Lists backup jobs.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<BackupJobResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListBackupsQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<BackupJobResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Starts a backup job.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<BackupJobResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Start(
        [FromBody] StartBackupRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new StartBackupCommand
            {
                BackupType = request?.BackupType ?? "Database",
                Scheduled = request?.Scheduled ?? false,
            },
            cancellationToken);
        return CreatedAtAction(nameof(List), ApiResponse<BackupJobResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Restores from a backup.</summary>
    [HttpPost("{id:guid}/restore")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Restore(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new RestoreBackupCommand { BackupJobId = id }, cancellationToken);
        return NoContent();
    }

    private string? GetCorrelationId() => HttpContext.Items["CorrelationId"]?.ToString();
}
