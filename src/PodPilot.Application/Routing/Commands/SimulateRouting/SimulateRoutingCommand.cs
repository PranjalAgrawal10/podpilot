using MediatR;
using PodPilot.Contracts.Routing;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Routing.Commands.SimulateRouting;

/// <summary>Simulates intelligent routing for a prompt.</summary>
public sealed class SimulateRoutingCommand : IRequest<SimulateRoutingResponse>
{
    /// <summary>Gets or sets the prompt.</summary>
    public string Prompt { get; init; } = string.Empty;

    /// <summary>Gets or sets an optional strategy override.</summary>
    public RoutingStrategy? Strategy { get; init; }

    /// <summary>Gets or sets an optional model hint.</summary>
    public string? ModelHint { get; init; }

    /// <summary>Gets or sets an optional path hint.</summary>
    public string? Path { get; init; }
}
