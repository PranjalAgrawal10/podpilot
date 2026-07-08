using MediatR;
using PodPilot.Contracts.Models;

namespace PodPilot.Application.Models.Queries.GetDownloads;

/// <summary>
/// Lists model downloads for the current organization.
/// </summary>
public sealed class GetDownloadsQuery : IRequest<IReadOnlyList<ModelDownloadResponse>>
{
    /// <summary>
    /// Gets or sets a value indicating whether to include only active downloads.
    /// </summary>
    public bool ActiveOnly { get; set; } = true;
}
