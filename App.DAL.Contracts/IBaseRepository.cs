using Base.Contracts;

namespace App.DAL.Contracts;

public interface IBaseRepository<TEntity>
    where TEntity : class, IBaseEntity
{
    IEnumerable<TEntity> All();
    Task<IEnumerable<TEntity>> AllAsync();

    TEntity? Find(params object[] id);
    Task<TEntity?> FindAsync(params object[] id);

    TEntity Add(TEntity entity);
    Task<TEntity> AddAsync(TEntity entity);

    TEntity Update(TEntity entity);

    void Remove(TEntity entity);
    Task<bool> RemoveAsync(Guid id, Guid? userId = null);

    Task<bool> ExistsAsync(Guid id, Guid? userId = null);
}
