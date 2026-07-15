using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Security;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Security;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// Organization secret catalog endpoints (metadata only).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/secrets")]
[Produces("application/json")]
public sealed class SecretsController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecretsController"/> class.
    /// </summary>
    public SecretsController(IMediator mediator) => this.mediator = mediator;

    /// <summary>Lists secrets.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SecretResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListSecretsQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SecretResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Creates a secret.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SecretResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateSecretRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateSecretCommand
            {
                Name = request.Name,
                SecretKind = request.SecretKind,
                BackendKind = request.BackendKind,
                Value = request.Value,
                ExpiresAt = request.ExpiresAt,
            },
            cancellationToken);
        return CreatedAtAction(nameof(List), ApiResponse<SecretResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Updates a secret.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SecretResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSecretRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateSecretCommand
            {
                SecretId = id,
                Name = request.Name,
                Value = request.Value,
                ExpiresAt = request.ExpiresAt,
                IsEnabled = request.IsEnabled,
            },
            cancellationToken);
        return Ok(ApiResponse<SecretResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Deletes a secret.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteSecretCommand { SecretId = id }, cancellationToken);
        return NoContent();
    }

    private string? GetCorrelationId() => HttpContext.Items["CorrelationId"]?.ToString();
}
