using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Commercial;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Commercial;

/// <summary>
/// Tracks organization onboarding wizard progress.
/// </summary>
public sealed class OnboardingService : IOnboardingService
{
    private readonly IApplicationDbContext db;

    /// <summary>
    /// Initializes a new instance of the <see cref="OnboardingService"/> class.
    /// </summary>
    public OnboardingService(IApplicationDbContext db) => this.db = db;

    /// <inheritdoc />
    public async Task<OnboardingStatus> GetAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var progress = await GetOrCreateAsync(organizationId, cancellationToken);
        return Map(progress);
    }

    /// <inheritdoc />
    public async Task<OnboardingStatus> CompleteStepAsync(
        Guid organizationId,
        OnboardingStep step,
        CancellationToken cancellationToken = default)
    {
        var progress = await GetOrCreateAsync(organizationId, cancellationToken);
        var completed = ParseSteps(progress.CompletedStepsJson);
        if (!completed.Contains(step))
        {
            completed.Add(step);
        }

        progress.CompletedStepsJson = JsonSerializer.Serialize(completed.Select(s => s.ToString()).ToList());

        if (step == OnboardingStep.Completed || completed.Contains(OnboardingStep.Completed))
        {
            progress.CurrentStep = OnboardingStep.Completed;
            progress.CompletedAt = DateTime.UtcNow;
        }
        else
        {
            var next = Enum.GetValues<OnboardingStep>()
                .Where(s => s != OnboardingStep.Completed && !completed.Contains(s))
                .OrderBy(s => (int)s)
                .Cast<OnboardingStep?>()
                .FirstOrDefault();

            progress.CurrentStep = next ?? OnboardingStep.Completed;
            if (progress.CurrentStep == OnboardingStep.Completed)
            {
                progress.CompletedAt = DateTime.UtcNow;
                if (!completed.Contains(OnboardingStep.Completed))
                {
                    completed.Add(OnboardingStep.Completed);
                    progress.CompletedStepsJson = JsonSerializer.Serialize(completed.Select(s => s.ToString()).ToList());
                }
            }
        }

        progress.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return Map(progress);
    }

    /// <inheritdoc />
    public async Task DismissAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var progress = await GetOrCreateAsync(organizationId, cancellationToken);
        progress.IsDismissed = true;
        progress.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<OnboardingProgress> GetOrCreateAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        var progress = await db.OnboardingProgressRecords
            .FirstOrDefaultAsync(p => p.OrganizationId == organizationId, cancellationToken);

        if (progress is not null)
        {
            return progress;
        }

        progress = new OnboardingProgress
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            CurrentStep = OnboardingStep.CreateOrganization,
            CompletedStepsJson = "[]",
            CreatedAt = DateTime.UtcNow,
        };

        await db.AddOnboardingProgressAsync(progress, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return progress;
    }

    private static List<OnboardingStep> ParseSteps(string json)
    {
        try
        {
            var names = JsonSerializer.Deserialize<List<string>>(json) ?? [];
            return names
                .Select(n => Enum.TryParse<OnboardingStep>(n, true, out var step) ? step : (OnboardingStep?)null)
                .Where(s => s.HasValue)
                .Select(s => s!.Value)
                .ToList();
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static OnboardingStatus Map(OnboardingProgress progress)
    {
        var completed = ParseSteps(progress.CompletedStepsJson);
        return new OnboardingStatus
        {
            CurrentStep = progress.CurrentStep,
            CompletedSteps = completed,
            IsDismissed = progress.IsDismissed,
            IsComplete = progress.CurrentStep == OnboardingStep.Completed
                         || completed.Contains(OnboardingStep.Completed)
                         || progress.CompletedAt.HasValue,
        };
    }
}
