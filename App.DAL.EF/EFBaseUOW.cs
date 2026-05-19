using App.DAL.Contracts;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF;

// Lecture 21 EXACT: EFBaseUOW<TDbContext> owns save logic
public class EFBaseUOW<TDbContext>(TDbContext dataContext) : IBaseUOW
    where TDbContext : DbContext
{
    protected readonly TDbContext UowDbContext = dataContext;

    public virtual int SaveChanges() => UowDbContext.SaveChanges();
    public virtual async Task<int> SaveChangesAsync() => await UowDbContext.SaveChangesAsync();
}
