/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Linq.Expressions;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using HCL.CS.SF.Domain;
using HCL.CS.SF.DomainServices;

namespace HCL.CS.SF.Infrastructure.Data;

/// <summary>
/// Generic base repository providing standard CRUD operations for entities derived from <see cref="BaseEntity"/>.
/// Uses the <see cref="IApplicationDbContext"/> for database access and supports soft-delete,
/// row-version concurrency, filtered queries with eager loading, and split-query optimization.
/// </summary>
/// <typeparam name="TEntity">The entity type, which must derive from <see cref="BaseEntity"/>.</typeparam>
internal class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
    private readonly IApplicationDbContext context;

    private bool disposed;
    private DbSet<TEntity> entities;

    /// <summary>
    /// Initializes a new instance of <see cref="BaseRepository{TEntity}"/> with the given database context.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public BaseRepository(IApplicationDbContext context)
    {
        this.context = context;
        entities = context.Set<TEntity>();
    }

    /// <summary>
    /// Inserts a single entity into the change tracker with <c>IsDeleted</c> set to false.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public virtual Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.IsDeleted = false;
        entities.Add(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Inserts a list of entities into the change tracker with <c>IsDeleted</c> set to false.
    /// </summary>
    /// <param name="entityList">The entities to insert.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public virtual Task InsertAsync(IList<TEntity> entityList, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entityList)
        {
            entity.IsDeleted = false;
            entities.Add(entity);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates a single entity with row-version concurrency control.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    public virtual Task UpdateAsync(TEntity entity)
    {
        entities.Update(entity);
        context.SetRowVersionStatus(entity, entity.RowVersion);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Performs a partial update on an entity, marking only the specified properties as modified.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="affectedProperties">The names of the properties that have changed.</param>
    public virtual Task UpdateAsync(TEntity entity, string[] affectedProperties)
    {
        entities.Attach(entity);
        foreach (var property in affectedProperties) context.SetPropertyModifiedStatus(entity, property);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates a list of entities with row-version concurrency control.
    /// </summary>
    /// <param name="entityList">The entities to update.</param>
    public virtual Task UpdateAsync(IList<TEntity> entityList)
    {
        foreach (var entity in entityList)
        {
            entities.Update(entity);
            context.SetRowVersionStatus(entity, entity.RowVersion);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes an entity by its primary key identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await entities.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null) entities.Remove(entity);
    }

    /// <summary>
    /// Deletes a single entity with row-version concurrency control.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    public virtual Task DeleteAsync(TEntity entity)
    {
        context.SetRowVersionStatus(entity, entity.RowVersion);
        entities.Remove(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes a list of entities with row-version concurrency control.
    /// </summary>
    /// <param name="entityList">The entities to delete.</param>
    public virtual Task DeleteAsync(IList<TEntity> entityList)
    {
        foreach (var entity in entityList)
        {
            context.SetRowVersionStatus(entity, entity.RowVersion);
            entities.Remove(entity);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves all non-deleted entities with optional eager loading. Uses split queries
    /// when more than one include is specified to avoid Cartesian explosion.
    /// </summary>
    /// <param name="includes">Optional navigation properties to eagerly load.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of all matching entities.</returns>
    public virtual async Task<IList<TEntity>> GetAllAsync(Expression<Func<TEntity, object>>[] includes = null, CancellationToken cancellationToken = default)
    {
        var includeList = includes ?? Array.Empty<Expression<Func<TEntity, object>>>();
        IQueryable<TEntity> query = entities;
        if (includeList.Length > 0)
        {
            foreach (var include in includeList) query = query.Include(include);

            if (includeList.Length > 1) query = query.AsSplitQuery();
        }

        return await query.AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Returns a no-tracking queryable for all non-deleted entities, allowing callers to compose additional LINQ.
    /// </summary>
    public virtual Task<IQueryable<TEntity>> GetAllForQueryAsync()
    {
        return Task.FromResult(entities.AsNoTracking().AsQueryable());
    }

    /// <summary>
    /// Retrieves a single entity by its primary key identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The entity if found; otherwise null.</returns>
    public virtual async Task<TEntity> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await entities.FindAsync(new object[] { id }, cancellationToken);
        if (result != null) return result;

        return null;
    }

    /// <summary>
    /// Retrieves entities matching a filter with optional eager loading and split-query support.
    /// </summary>
    /// <param name="filter">A predicate to filter entities.</param>
    /// <param name="includes">Optional navigation properties to eagerly load.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of matching entities, or null if filter is null.</returns>
    public virtual async Task<IList<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, object>>[] includes = null,
        CancellationToken cancellationToken = default)
    {
        if (filter != null)
        {
            var includeList = includes ?? Array.Empty<Expression<Func<TEntity, object>>>();
            IQueryable<TEntity> query = entities;

            if (includeList.Length > 0)
            {
                foreach (var include in includeList) query = query.Include(include);

                if (includeList.Length > 1) query = query.AsSplitQuery();
            }

            query = query.Where(filter);
            return await query.ToListAsync(cancellationToken);
        }

        return null;
    }

    /// <summary>
    /// Retrieves entities including soft-deleted records by ignoring the global query filter.
    /// Supports optional projection via a select expression.
    /// </summary>
    public virtual async Task<IList<TType>> GetWithSoftDeleteAsync<TType>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TType>> select = null,
        CancellationToken cancellationToken = default) where TType : class
    {
        if (filter != null)
        {
            var query = entities.IgnoreQueryFilters();
            query = query.Where(filter);
            if (select != null) return await query.Select(select).ToListAsync(cancellationToken);

            return (IList<TType>)await query.ToListAsync(cancellationToken);
        }

        return null;
    }

    /// <summary>
    /// Retrieves entities with full query composition: filtering, projection, ordering,
    /// and eager loading with split-query support.
    /// </summary>
    /// <typeparam name="TType">The projected result type.</typeparam>
    /// <param name="filter">Optional predicate to filter entities.</param>
    /// <param name="select">Optional projection expression.</param>
    /// <param name="orderBy">Optional ordering function.</param>
    /// <param name="includes">Optional navigation properties to eagerly load.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of projected results.</returns>
    //example: var users = unitOfWork.UserRepository.Get(
    //filter: u => u.RoleId == 2 & u.DepartmentId == 1,
    //orderBy: u => u.UserName,
    //selector: u => u.Select(m => new { m.UserName, m.RoleId }),
    //includeProperties: "Roles,Departments");
    public virtual async Task<IList<TType>> GetAsync<TType>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TType>> select = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        Expression<Func<TEntity, object>>[] includes = null,
        CancellationToken cancellationToken = default) where TType : class
    {
        var includeList = includes ?? Array.Empty<Expression<Func<TEntity, object>>>();
        IQueryable<TEntity> query = entities;
        if (filter != null) query = query.Where(filter);

        if (includeList.Length > 0)
        {
            foreach (var include in includeList) query = query.Include(include);

            if (includeList.Length > 1) query = query.AsSplitQuery();
        }

        if (orderBy != null)
        {
            if (select != null) return await orderBy(query).Select(select).ToListAsync(cancellationToken);

            return (IList<TType>)await orderBy(query).ToListAsync(cancellationToken);
        }

        if (select != null) return await query.Select(select).ToListAsync(cancellationToken);

        return (IList<TType>)await query.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Checks whether any active (non-deleted) entity matches the given filter.
    /// </summary>
    /// <param name="filter">The predicate to evaluate.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if at least one matching active entity exists.</returns>
    public virtual async Task<bool> ActiveRecordExistsAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        if (filter != null)
        {
            IQueryable<TEntity> query = entities;
            query = query.Where(filter);
            return await query.AnyAsync(cancellationToken);
        }

        return false;
    }

    /// <summary>
    /// Checks whether any entity (including soft-deleted) matches the given filter.
    /// Used to detect duplicates across both active and deleted records.
    /// </summary>
    /// <param name="filter">The predicate to evaluate.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if at least one matching entity exists (ignoring query filters).</returns>
    public virtual async Task<bool> DuplicateExistsAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        if (filter != null)
        {
            var query = entities.IgnoreQueryFilters();
            query = query.Where(filter);
            return await query.AnyAsync(cancellationToken);
        }

        return false;
    }

    /// <summary>
    /// Persists all pending changes using the context's soft-delete save mechanism.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Persists all pending changes using the context's hard-delete save mechanism.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> SaveChangesWithHardDeleteAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesWithHardDeleteAsync(cancellationToken);
    }

    /// <summary>
    /// Releases all resources used by this repository.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the DbSet reference when disposing.
    /// </summary>
    /// <param name="disposing">True if called from Dispose(); false if called from a finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
            if (disposing)
                if (entities != null)
                    entities = null;

        disposed = true;
    }
}
