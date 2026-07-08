using MediatR;
using PodPilot.Contracts.Models;

namespace PodPilot.Application.Models.Queries.GetModelDetails;

/// <summary>
/// Gets detailed information for a model.
/// </summary>
public sealed class GetModelDetailsQuery : IRequest<ModelDetailResponse>
{
    /// <summary>
    /// Gets or sets the model identifier.
    /// </summary>
    public Guid ModelId { get; set; }
}
