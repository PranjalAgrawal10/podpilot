using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Application.Providers.Commands.CreateProvider;
using PodPilot.Application.Providers.Commands.DeleteProvider;
using PodPilot.Application.Providers.Commands.UpdateProvider;
using PodPilot.Application.Providers.Commands.ValidateCredentials;
using PodPilot.Application.Providers.Commands.ValidateProvider;
using PodPilot.Application.Providers.Queries.GetProvider;
using PodPilot.Application.Providers.Queries.GetProviderHealth;
using PodPilot.Application.Providers.Queries.ListProviderGpus;
using PodPilot.Application.Providers.Queries.ListProviderRegions;
using PodPilot.Application.Providers.Queries.ListProviders;
using PodPilot.Application.Providers.Queries.ListProviderTemplates;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Providers;
using PodPilot.Domain.Enums;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// Compute provider management endpoints.
/// </summary>
[ApiController]
[Route("api/v1/providers")]
[Authorize]
[Produces("application/json")]
public sealed class ProvidersController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProvidersController"/> class.
    /// </summary>
    /// <param name="mediator">The MediatR mediator.</param>
    public ProvidersController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    /// <summary>
    /// Lists compute providers for the current organization.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ProviderResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListProviders(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListProvidersQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ProviderResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Gets a compute provider by identifier.
    /// </summary>
    [HttpGet("{providerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProviderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProvider(Guid providerId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProviderQuery { ProviderId = providerId }, cancellationToken);
        return Ok(ApiResponse<ProviderResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Creates a compute provider.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProviderResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateProvider(
        [FromBody] CreateProviderRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ProviderType>(request.ProviderType, ignoreCase: true, out var providerType))
        {
            throw new ValidationException($"Provider type '{request.ProviderType}' is invalid.");
        }

        var result = await mediator.Send(
            new CreateProviderCommand
            {
                Name = request.Name,
                ProviderType = providerType,
                DisplayName = request.DisplayName,
                Description = request.Description,
                DefaultRegion = request.DefaultRegion,
                ApiKey = request.ApiKey,
                IsEnabled = request.IsEnabled,
            },
            cancellationToken);

        return CreatedAtAction(
            nameof(GetProvider),
            new { providerId = result.Id },
            ApiResponse<ProviderResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Updates a compute provider.
    /// </summary>
    [HttpPut("{providerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProviderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProvider(
        Guid providerId,
        [FromBody] UpdateProviderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateProviderCommand
            {
                ProviderId = providerId,
                Name = request.Name,
                DisplayName = request.DisplayName,
                Description = request.Description,
                DefaultRegion = request.DefaultRegion,
                ApiKey = request.ApiKey,
                IsEnabled = request.IsEnabled,
            },
            cancellationToken);

        return Ok(ApiResponse<ProviderResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Deletes a compute provider.
    /// </summary>
    [HttpDelete("{providerId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteProvider(Guid providerId, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteProviderCommand { ProviderId = providerId }, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Validates provider credentials before creation.
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ApiResponse<ProviderValidationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateCredentials(
        [FromBody] ValidateCredentialsRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ProviderType>(request.ProviderType, ignoreCase: true, out var providerType))
        {
            throw new ValidationException($"Provider type '{request.ProviderType}' is invalid.");
        }

        var result = await mediator.Send(
            new ValidateCredentialsCommand
            {
                ProviderType = providerType,
                ApiKey = request.ApiKey,
            },
            cancellationToken);

        return Ok(ApiResponse<ProviderValidationResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Validates provider credentials.
    /// </summary>
    [HttpPost("{providerId:guid}/validate")]
    [ProducesResponseType(typeof(ApiResponse<ProviderValidationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateProvider(
        Guid providerId,
        [FromBody] ValidateProviderRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ValidateProviderCommand
            {
                ProviderId = providerId,
                ApiKey = request?.ApiKey,
            },
            cancellationToken);

        return Ok(ApiResponse<ProviderValidationResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Lists regions for a compute provider.
    /// </summary>
    [HttpGet("{providerId:guid}/regions")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ProviderRegionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListRegions(Guid providerId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ListProviderRegionsQuery { ProviderId = providerId },
            cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<ProviderRegionResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Lists GPUs for a compute provider.
    /// </summary>
    [HttpGet("{providerId:guid}/gpus")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ProviderGpuResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListGpus(Guid providerId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ListProviderGpusQuery { ProviderId = providerId },
            cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<ProviderGpuResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Lists templates for a compute provider.
    /// </summary>
    [HttpGet("{providerId:guid}/templates")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ProviderTemplateResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListTemplates(Guid providerId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ListProviderTemplatesQuery { ProviderId = providerId },
            cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<ProviderTemplateResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>
    /// Gets health information for a compute provider.
    /// </summary>
    [HttpGet("{providerId:guid}/health")]
    [ProducesResponseType(typeof(ApiResponse<ProviderHealthResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHealth(Guid providerId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetProviderHealthQuery { ProviderId = providerId },
            cancellationToken);

        return Ok(ApiResponse<ProviderHealthResponse>.Ok(result, GetCorrelationId()));
    }

    private string? GetCorrelationId() =>
        HttpContext.Items["CorrelationId"]?.ToString();
}
