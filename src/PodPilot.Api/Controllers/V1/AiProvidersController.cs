using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodPilot.Application.AiProviders.Commands.CreateAiProvider;
using PodPilot.Application.AiProviders.Commands.CreateRoutingPolicy;
using PodPilot.Application.AiProviders.Commands.DeleteAiProvider;
using PodPilot.Application.AiProviders.Commands.DeleteRoutingPolicy;
using PodPilot.Application.AiProviders.Commands.UpdateAiProvider;
using PodPilot.Application.AiProviders.Commands.UpdateRoutingPolicy;
using PodPilot.Application.AiProviders.Commands.ValidateAiProvider;
using PodPilot.Application.AiProviders.Queries.GetAiProvider;
using PodPilot.Application.AiProviders.Queries.GetAiProviderDashboard;
using PodPilot.Application.AiProviders.Queries.GetAiProviderHealth;
using PodPilot.Application.AiProviders.Queries.ListAiProviderKinds;
using PodPilot.Application.AiProviders.Queries.ListAiProviderModels;
using PodPilot.Application.AiProviders.Queries.ListAiProviders;
using PodPilot.Application.AiProviders.Queries.ListRoutingPolicies;
using PodPilot.Application.Common.Exceptions;
using PodPilot.Contracts.AiProviders;
using PodPilot.Contracts.Common;
using PodPilot.Domain.Enums;

namespace PodPilot.Api.Controllers.V1;

/// <summary>
/// AI inference provider management endpoints.
/// </summary>
[ApiController]
[Authorize]
[Produces("application/json")]
public sealed class AiProvidersController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiProvidersController"/> class.
    /// </summary>
    public AiProvidersController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    /// <summary>Lists AI providers.</summary>
    [HttpGet("api/v1/ai/providers")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AiProviderResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListProviders(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListAiProvidersQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AiProviderResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Gets an AI provider.</summary>
    [HttpGet("api/v1/ai/providers/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AiProviderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProvider(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAiProviderQuery { ProviderId = id }, cancellationToken);
        return Ok(ApiResponse<AiProviderResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Creates an AI provider.</summary>
    [HttpPost("api/v1/ai/providers")]
    [ProducesResponseType(typeof(ApiResponse<AiProviderResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateProvider(
        [FromBody] CreateAiProviderRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AiProviderKind>(request.ProviderKind, ignoreCase: true, out var providerKind))
        {
            throw new ValidationException($"Provider kind '{request.ProviderKind}' is invalid.");
        }

        var result = await mediator.Send(
            new CreateAiProviderCommand
            {
                Name = request.Name,
                DisplayName = request.DisplayName,
                Description = request.Description,
                ProviderKind = providerKind,
                ApiKey = request.ApiKey,
                BaseUrl = request.BaseUrl,
                DeploymentName = request.DeploymentName,
                ApiVersion = request.ApiVersion,
                IsEnabled = request.IsEnabled,
                Priority = request.Priority,
            },
            cancellationToken);

        return CreatedAtAction(
            nameof(GetProvider),
            new { id = result.Id },
            ApiResponse<AiProviderResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Updates an AI provider.</summary>
    [HttpPut("api/v1/ai/providers/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AiProviderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProvider(
        Guid id,
        [FromBody] UpdateAiProviderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateAiProviderCommand
            {
                ProviderId = id,
                Name = request.Name,
                DisplayName = request.DisplayName,
                Description = request.Description,
                ApiKey = request.ApiKey,
                BaseUrl = request.BaseUrl,
                DeploymentName = request.DeploymentName,
                ApiVersion = request.ApiVersion,
                IsEnabled = request.IsEnabled,
                Priority = request.Priority,
            },
            cancellationToken);

        return Ok(ApiResponse<AiProviderResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Deletes an AI provider.</summary>
    [HttpDelete("api/v1/ai/providers/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteProvider(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteAiProviderCommand { ProviderId = id }, cancellationToken);
        return NoContent();
    }

    /// <summary>Validates AI provider credentials.</summary>
    [HttpPost("api/v1/ai/providers/{id:guid}/validate")]
    [ProducesResponseType(typeof(ApiResponse<AiProviderValidationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateProvider(
        Guid id,
        [FromBody] ValidateAiProviderRequest? request,
        CancellationToken cancellationToken)
    {
        AiProviderKind providerKind = default;
        if (request is not null &&
            !string.IsNullOrWhiteSpace(request.ProviderKind) &&
            !Enum.TryParse(request.ProviderKind, ignoreCase: true, out providerKind))
        {
            throw new ValidationException($"Provider kind '{request.ProviderKind}' is invalid.");
        }

        var provider = await mediator.Send(new GetAiProviderQuery { ProviderId = id }, cancellationToken);
        if (!Enum.TryParse(provider.ProviderKind, ignoreCase: true, out providerKind))
        {
            throw new ValidationException($"Provider kind '{provider.ProviderKind}' is invalid.");
        }

        var result = await mediator.Send(
            new ValidateAiProviderCommand
            {
                ProviderId = id,
                ProviderKind = providerKind,
                ApiKey = request?.ApiKey ?? string.Empty,
                BaseUrl = request?.BaseUrl,
                DeploymentName = request?.DeploymentName,
                ApiVersion = request?.ApiVersion,
            },
            cancellationToken);

        return Ok(ApiResponse<AiProviderValidationResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Gets AI provider health.</summary>
    [HttpGet("api/v1/ai/providers/{id:guid}/health")]
    [ProducesResponseType(typeof(ApiResponse<AiProviderHealthResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProviderHealth(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAiProviderHealthQuery { ProviderId = id }, cancellationToken);
        return Ok(ApiResponse<AiProviderHealthResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Lists AI provider models.</summary>
    [HttpGet("api/v1/ai/models")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AiProviderModelResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListModels([FromQuery] Guid? providerId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListAiProviderModelsQuery { ProviderId = providerId }, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AiProviderModelResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Lists routing policies.</summary>
    [HttpGet("api/v1/ai/routing-policies")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AiRoutingPolicyResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListRoutingPolicies(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListRoutingPoliciesQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AiRoutingPolicyResponse>>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Creates a routing policy.</summary>
    [HttpPost("api/v1/ai/routing-policies")]
    [ProducesResponseType(typeof(ApiResponse<AiRoutingPolicyResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateRoutingPolicy(
        [FromBody] CreateAiRoutingPolicyRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AiFailoverStrategy>(request.FailoverStrategy, ignoreCase: true, out var strategy))
        {
            throw new ValidationException($"Failover strategy '{request.FailoverStrategy}' is invalid.");
        }

        var result = await mediator.Send(
            new CreateRoutingPolicyCommand
            {
                Name = request.Name,
                ModelName = request.ModelName,
                PrimaryProviderId = request.PrimaryProviderId,
                FallbackProviderIds = request.FallbackProviderIds,
                FailoverStrategy = strategy,
                MaxRetries = request.MaxRetries,
                IsEnabled = request.IsEnabled,
                IsDefault = request.IsDefault,
            },
            cancellationToken);

        return CreatedAtAction(
            nameof(ListRoutingPolicies),
            ApiResponse<AiRoutingPolicyResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Updates a routing policy.</summary>
    [HttpPut("api/v1/ai/routing-policies/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AiRoutingPolicyResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateRoutingPolicy(
        Guid id,
        [FromBody] UpdateAiRoutingPolicyRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AiFailoverStrategy>(request.FailoverStrategy, ignoreCase: true, out var strategy))
        {
            throw new ValidationException($"Failover strategy '{request.FailoverStrategy}' is invalid.");
        }

        var result = await mediator.Send(
            new UpdateRoutingPolicyCommand
            {
                PolicyId = id,
                Name = request.Name,
                ModelName = request.ModelName,
                PrimaryProviderId = request.PrimaryProviderId,
                FallbackProviderIds = request.FallbackProviderIds,
                FailoverStrategy = strategy,
                MaxRetries = request.MaxRetries,
                IsEnabled = request.IsEnabled,
                IsDefault = request.IsDefault,
            },
            cancellationToken);

        return Ok(ApiResponse<AiRoutingPolicyResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Deletes a routing policy.</summary>
    [HttpDelete("api/v1/ai/routing-policies/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteRoutingPolicy(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteRoutingPolicyCommand { PolicyId = id }, cancellationToken);
        return NoContent();
    }

    /// <summary>Gets the AI provider dashboard.</summary>
    [HttpGet("api/v1/ai/dashboard")]
    [ProducesResponseType(typeof(ApiResponse<AiProviderDashboardResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAiProviderDashboardQuery(), cancellationToken);
        return Ok(ApiResponse<AiProviderDashboardResponse>.Ok(result, GetCorrelationId()));
    }

    /// <summary>Lists supported AI provider kinds.</summary>
    [HttpGet("api/v1/ai/provider-kinds")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AiProviderKindMetadataResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListProviderKinds(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListAiProviderKindsQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AiProviderKindMetadataResponse>>.Ok(result, GetCorrelationId()));
    }

    private string? GetCorrelationId() =>
        HttpContext.Items.TryGetValue("CorrelationId", out var value) ? value?.ToString() : HttpContext.TraceIdentifier;
}
