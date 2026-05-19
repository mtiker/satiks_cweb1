using App.DAL.Contracts;
using Base.Contracts;
using Base.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.DAL.EF.Repositories;

public class EFBaseRepository<TEntity>(AppDbContext dataContext) : IBaseRepository<TEntity> where TEntity : class, IBaseEntity
{
    protected readonly AppDbContext RepositoryDbContext = dataContext;
    protected readonly DbSet<TEntity> RepositoryDbSet = dataContext.Set<TEntity>();

    public virtual IEnumerable<TEntity> All() => RepositoryDbSet.ToList();

    public virtual async Task<IEnumerable<TEntity>> AllAsync()
        => await RepositoryDbSet.ToListAsync();

    public virtual TEntity? Find(params object[] id) => RepositoryDbSet.Find(id);

    public virtual async Task<TEntity?> FindAsync(params object[] id)
        => await RepositoryDbSet.FindAsync(id);

    public virtual TEntity Add(TEntity entity) => RepositoryDbSet.Add(entity).Entity;

    public virtual async Task<TEntity> AddAsync(TEntity entity)
        => (await RepositoryDbSet.AddAsync(entity)).Entity;

    public virtual TEntity Update(TEntity entity)
    {
        // BaseEntity is the concrete base — using directive keeps this clean
        if (entity is BaseEntity be) be.UpdatedAt = DateTime.UtcNow;
        return RepositoryDbSet.Update(entity).Entity;
    }

    public virtual void Remove(TEntity entity) => RepositoryDbSet.Remove(entity);

    public virtual async Task<bool> RemoveAsync(Guid id, Guid? userId = null)
    {
        var entity = await RepositoryDbSet.FindAsync(id);
        if (entity == null) return false;
        Remove(entity);
        return true;
    }

    public virtual async Task<bool> ExistsAsync(Guid id, Guid? userId = null)
        => await RepositoryDbSet.AnyAsync(e => e.Id == id);
}
