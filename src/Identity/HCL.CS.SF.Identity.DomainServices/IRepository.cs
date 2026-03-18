/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Linq.Expressions;
using System.Threading;
using HCL.CS.SF.Domain;

namespace HCL.CS.SF.DomainServices;

/// <summary>
/// Generic repository contract providing standard CRUD, query, and persistence operations
/// for domain entities. Implementations should apply soft-delete filtering by default and
/// delegate persistence to <see cref="IApplicationDbContext"/>.
/// </summary>
/// <typeparam name="TEntity">The domain entity type, which must derive from <see cref="BaseEntity"/>.</typeparam>
public interface IRepository<TEntity> : IDisposable where TEntity : BaseEntity
{
    /// <summary>Inserts a single entity into the repository.</summary>
    /// <param name="entity">The entity to insert.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>Inserts multiple entities into the repository in a single operation.</summary>
    /// <param name="entityList">The entities to insert.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task InsertAsync(IList<TEntity> entityList, CancellationToken cancellationToken = default);

    /// <summary>Updates all properties of the specified entity.</summary>
    /// <param name="entity">The entity with updated values.</param>
    Task UpdateAsync(TEntity entity);

    /// <summary>Updates only the specified properties of the entity (partial update).</summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="affectedProperties">The names of properties that changed.</param>
    Task UpdateAsync(TEntity entity, string[] affectedProperties);

    /// <summary>Updates multiple entities in a single operation.</summary>
    /// <param name="entityList">The entities to update.</param>
    Task UpdateAsync(IList<TEntity> entityList);

    /// <summary>Soft-deletes the entity identified by the given ID.</summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Soft-deletes the specified entity.</summary>
    /// <param name="entity">The entity to delete.</param>
    Task DeleteAsync(TEntity entity);

    /// <summary>Soft-deletes multiple entities in a single operation.</summary>
    /// <param name="entityList">The entities to delete.</param>
    Task DeleteAsync(IList<TEntity> entityList);

    /// <summary>Retrieves all active (non-deleted) entities, optionally including related navigation properties.</summary>
    /// <param name="includes">Optional array of navigation property expressions to eager-load.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<IList<TEntity>> GetAllAsync(Expression<Func<TEntity, object>>[] includes = null, CancellationToken cancellationToken = default);

    /// <summary>Returns an <see cref="IQueryable{T}"/> for building custom queries against active entities.</summary>
    Task<IQueryable<TEntity>> GetAllForQueryAsync();

    /// <summary>Retrieves a single entity by its unique identifier.</summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<TEntity> GetAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Retrieves entities matching a filter, optionally including related navigation properties.</summary>
    /// <param name="filter">A predicate expression to filter entities.</param>
    /// <param name="includes">Optional array of navigation property expressions to eager-load.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<IList<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, object>>[] includes = null,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves a projected list of entities matching a filter, with optional ordering and includes.</summary>
    /// <typeparam name="TType">The projected result type.</typeparam>
    /// <param name="filter">A predicate expression to filter entities.</param>
    /// <param name="select">A projection expression to shape results.</param>
    /// <param name="orderBy">An optional ordering function.</param>
    /// <param name="includes">Optional array of navigation property expressions to eager-load.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<IList<TType>> GetAsync<TType>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TType>> select = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        Expression<Func<TEntity, object>>[] includes = null,
        CancellationToken cancellationToken = default) where TType : class;

    /// <summary>Retrieves projected entities including soft-deleted records (bypasses the soft-delete filter).</summary>
    /// <typeparam name="TType">The projected result type.</typeparam>
    /// <param name="filter">A predicate expression to filter entities.</param>
    /// <param name="select">A projection expression to shape results.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<IList<TType>> GetWithSoftDeleteAsync<TType>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TType>> select = null,
        CancellationToken cancellationToken = default) where TType : class;

    /// <summary>Checks whether an active (non-deleted) record matching the filter exists.</summary>
    /// <param name="filter">A predicate expression to match.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<bool> ActiveRecordExistsAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);

    /// <summary>Checks whether a duplicate record (including soft-deleted) matching the filter exists.</summary>
    /// <param name="filter">A predicate expression to match.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<bool> DuplicateExistsAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);

    /// <summary>Persists all pending changes using soft-delete semantics.</summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<FrameworkResult> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Persists all pending changes using hard-delete semantics (physical removal).</summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<FrameworkResult> SaveChangesWithHardDeleteAsync(CancellationToken cancellationToken = default);
}
