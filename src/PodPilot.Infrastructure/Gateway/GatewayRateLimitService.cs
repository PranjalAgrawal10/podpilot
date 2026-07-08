using System.Collections.Concurrent;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Gateway;

namespace PodPilot.Infrastructure.Gateway;

/// <summary>
/// Enforces per-key and per-organization gateway rate limits.
/// </summary>
public interface IGatewayRateLimitService
{
    /// <summary>
    /// Attempts to acquire a rate limit slot.
    /// </summary>
    GatewayRateLimitResult TryAcquire(GatewayAuthContext auth);
}

/// <summary>
/// Result of a rate limit check.
/// </summary>
public sealed class GatewayRateLimitResult
{
    /// <summary>
    /// Gets or sets whether the request is allowed.
    /// </summary>
    public bool Allowed { get; init; }

    /// <summary>
    /// Gets or sets the retry-after seconds when denied.
    /// </summary>
    public int RetryAfterSeconds { get; init; }
}

/// <summary>
/// In-memory gateway rate limiter.
/// </summary>
public sealed class GatewayRateLimitService : IGatewayRateLimitService
{
    private readonly ConcurrentDictionary<string, RateWindow> windows = new();

    /// <inheritdoc />
    public GatewayRateLimitResult TryAcquire(GatewayAuthContext auth)
    {
        var now = DateTime.UtcNow;
        var minuteKey = $"key:{auth.ApiKeyId}:minute:{now:yyyyMMddHHmm}";
        var dayKey = $"key:{auth.ApiKeyId}:day:{now:yyyyMMdd}";
        var orgMinuteKey = $"org:{auth.OrganizationId}:minute:{now:yyyyMMddHHmm}";
        var orgDayKey = $"org:{auth.OrganizationId}:day:{now:yyyyMMdd}";

        if (!TryIncrement(minuteKey, auth.RateLimitPerMinute, TimeSpan.FromMinutes(1), now))
        {
            return new GatewayRateLimitResult { Allowed = false, RetryAfterSeconds = 60 };
        }

        if (!TryIncrement(dayKey, auth.RateLimitPerDay, TimeSpan.FromDays(1), now))
        {
            return new GatewayRateLimitResult { Allowed = false, RetryAfterSeconds = 3600 };
        }

        if (!TryIncrement(orgMinuteKey, auth.RateLimitPerMinute * 5, TimeSpan.FromMinutes(1), now))
        {
            return new GatewayRateLimitResult { Allowed = false, RetryAfterSeconds = 60 };
        }

        if (!TryIncrement(orgDayKey, auth.RateLimitPerDay * 5, TimeSpan.FromDays(1), now))
        {
            return new GatewayRateLimitResult { Allowed = false, RetryAfterSeconds = 3600 };
        }

        return new GatewayRateLimitResult { Allowed = true };
    }

    private bool TryIncrement(string key, int limit, TimeSpan window, DateTime now)
    {
        var entry = windows.AddOrUpdate(
            key,
            _ => new RateWindow(1, now.Add(window)),
            (_, existing) =>
            {
                if (existing.ExpiresAt <= now)
                {
                    return new RateWindow(1, now.Add(window));
                }

                return new RateWindow(existing.Count + 1, existing.ExpiresAt);
            });

        return entry.Count <= limit;
    }

    private sealed class RateWindow
    {
        public RateWindow(int count, DateTime expiresAt)
        {
            Count = count;
            ExpiresAt = expiresAt;
        }

        public int Count { get; }

        public DateTime ExpiresAt { get; }
    }
}
