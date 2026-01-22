using CoachBoard.Application.Interfaces;
using CoachBoard.Infrastructure.Persistence;
using CoachBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CoachBoard.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly CoachBoardDbContext _context;
    protected readonly ICurrentTenant _currentTenant;
    private readonly DbSet<T> _dbSet;

    public Repository(CoachBoardDbContext context, ICurrentTenant currentTenant)
    {
        _context = context;
        _currentTenant = currentTenant;
        _dbSet = context.Set<T>();
    }

    protected IQueryable<T> GetQuery()
    {
        var query = _dbSet.AsQueryable();
        
        if (typeof(ITenantEntity).IsAssignableFrom(typeof(T)))
        {
            var tenantId = _currentTenant.TenantId ?? 0;
            query = query.Where(x => ((ITenantEntity)x).TenantId == tenantId);
        }

        return query;
    }

    public async Task<IEnumerable<T>> GetAllAsync() =>
        await GetQuery().AsNoTracking().ToListAsync();

    public async Task<T?> GetByIdAsync(int id)
    {
        // FindAsync doesn't support IQueryable filtering directly, so we use FirstOrDefaultAsync
        // to ensure the tenant filter is applied. 
        if (typeof(ITenantEntity).IsAssignableFrom(typeof(T)))
        {
            return await GetQuery().FirstOrDefaultAsync(x => EF.Property<int>(x, "Id") == id);
        }
        
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await GetQuery().Where(predicate).ToListAsync();

    public async Task AddAsync(T entity)
    {
        if (entity is ITenantEntity tenantEntity)
        {
            tenantEntity.TenantId = _currentTenant.TenantId ?? 0;
        }
        await _dbSet.AddAsync(entity);
    }

    public async Task UpdateAsync(T entity)
    {
        // Ensure the entity belongs to the current tenant before updating
        if (entity is ITenantEntity tenantEntity)
        {
            var tenantId = _currentTenant.TenantId ?? 0;
            if (tenantEntity.TenantId != tenantId && tenantId != 0)
            {
                throw new UnauthorizedAccessException("Cannot update an entity belonging to another tenant.");
            }
        }
        _dbSet.Update(entity);
    }

    public async Task DeleteAsync(T entity)
    {
        // Ensure the entity belongs to the current tenant before deleting
        if (entity is ITenantEntity tenantEntity)
        {
            var tenantId = _currentTenant.TenantId ?? 0;
            if (tenantEntity.TenantId != tenantId && tenantId != 0)
            {
                throw new UnauthorizedAccessException("Cannot delete an entity belonging to another tenant.");
            }
        }
        _dbSet.Remove(entity);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
