using DAL.Context;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DAL.Repositories;

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<TEntity> _entities;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _entities = _context.Set<TEntity>();
    }

    public virtual async Task<List<TEntity>> GetAllAsync()
    {
        return await _entities.AsNoTracking().ToListAsync();
    }

    public virtual async Task<TEntity?> GetByIdAsync(int id)
    {
        return await _entities.FindAsync(id);
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        await _entities.AddAsync(entity);
        return entity;
    }

    public virtual Task<bool> UpdateAsync(TEntity entity)
    {
        _entities.Update(entity);
        return Task.FromResult(true);
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        var record = await GetByIdAsync(id);
        if (record == null)
            return false;

        _entities.Remove(record);
        return true;
    }

    public virtual async Task<List<TEntity>> GetPagedAsync(int pageNumber, int pageSize)
    {
        return await _entities
            .AsNoTracking()
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public virtual async Task<int> GetCountAsync()
    {
        return await _entities.CountAsync();
    }

    public virtual async Task<bool> IsExistAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _entities.AnyAsync(predicate);
    }

    public virtual async Task<bool> ChangeStatusAsync(int id, string? userId, string propertyName = "IsActive")
    {
        var entity = await _entities.FindAsync(id);
        if (entity == null)
            return false;

        var statusProp = typeof(TEntity).GetProperty(propertyName);
        if (statusProp == null)
            return false;

        var currentValue = (bool?)statusProp.GetValue(entity) ?? false;
        statusProp.SetValue(entity, !currentValue);

        var updatedByIdProp = typeof(TEntity).GetProperty("UpdatedById");
        if (updatedByIdProp != null && updatedByIdProp.CanWrite && !string.IsNullOrEmpty(userId))
        {
            updatedByIdProp.SetValue(entity, userId);
        }

        var updatedAtProp = typeof(TEntity).GetProperty("UpdatedAt");
        if (updatedAtProp != null && updatedAtProp.CanWrite)
        {
            updatedAtProp.SetValue(entity, DateTime.UtcNow);
        }

        _entities.Update(entity);
        return true;
    }

    public virtual async Task<List<TEntity>> AddRangeAsync(List<TEntity> entities)
    {
        await _entities.AddRangeAsync(entities);
        return entities;
    }

    public virtual Task<bool> UpdateRangeAsync(List<TEntity> entities)
    {
        _entities.UpdateRange(entities);
        return Task.FromResult(true);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(ISpecifications<TEntity> specs)
    {
        var query = SpecificationsEvaluator.BuildQuery(_entities.AsNoTracking(), specs);
        return await query.ToListAsync();
    }

    public virtual async Task<int> GetCountAsync(ISpecifications<TEntity> specs)
    {
        specs.DisablePagination();
        return await SpecificationsEvaluator.BuildQuery(_entities.AsNoTracking(), specs).CountAsync();
    }
}
