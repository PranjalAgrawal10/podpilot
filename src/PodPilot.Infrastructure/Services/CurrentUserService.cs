using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PodPilot.Application.Common.Interfaces;

namespace PodPilot.Infrastructure.Services;

/// <summary>
/// Provides current user information from HTTP context claims.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentUserService"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public Guid? UserId
    {
        get
        {
            var userId = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userId, out var id) ? id : null;
        }
    }

    /// <inheritdoc />
    public string? Email =>
        httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    /// <inheritdoc />
    public IReadOnlyList<string> Roles =>
        httpContextAccessor.HttpContext?.User?
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList() ?? [];

    /// <inheritdoc />
    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
