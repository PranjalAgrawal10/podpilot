using System.Linq.Expressions;
using PodPilot.Domain.Common;

namespace PodPilot.Domain.Interfaces;

/// <summary>
/// Generic repository contract for aggregate roots.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IRepository<T>
    where T : BaseEntity
{
    /// <summary>
    /// Gets an entity by identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The entity if found; otherwise <c>null</c>.</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities matching a predicate.
    /// </summary>
    /// <param name="predicate">The filter expression.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Matching entities.</returns>
    Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    void Update(T entity);

    /// <summary>
    /// Removes an entity.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    void Remove(T entity);
}
