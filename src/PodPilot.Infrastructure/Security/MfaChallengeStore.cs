using System.Collections.Concurrent;
using PodPilot.Application.Common.Interfaces;

namespace PodPilot.Infrastructure.Security;

/// <summary>
/// In-memory MFA challenge token store.
/// </summary>
public sealed class MfaChallengeStore : IMfaChallengeStore
{
    private readonly ConcurrentDictionary<string, (Guid UserId, DateTime ExpiresAtUtc)> store = new();

    /// <inheritdoc />
    public Task StoreAsync(string token, Guid userId, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        store[token] = (userId, DateTime.UtcNow.Add(ttl));
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<Guid?> PeekAsync(string token, CancellationToken cancellationToken = default)
    {
        if (store.TryGetValue(token, out var entry) && entry.ExpiresAtUtc > DateTime.UtcNow)
        {
            return Task.FromResult<Guid?>(entry.UserId);
        }

        return Task.FromResult<Guid?>(null);
    }

    /// <inheritdoc />
    public Task<Guid?> ConsumeAsync(string token, CancellationToken cancellationToken = default)
    {
        if (store.TryRemove(token, out var entry) && entry.ExpiresAtUtc > DateTime.UtcNow)
        {
            return Task.FromResult<Guid?>(entry.UserId);
        }

        return Task.FromResult<Guid?>(null);
    }
}
