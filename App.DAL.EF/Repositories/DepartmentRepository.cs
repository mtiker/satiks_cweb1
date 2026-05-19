using App.DAL.Contracts;
using App.Domain;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.DAL.EF.Repositories;

public class DepartmentRepository(AppDbContext context) : EFBaseRepository<Department>(context), IDepartmentRepository
{
    public async Task<IEnumerable<Department>> AllWithDetailsAsync()
        => await RepositoryDbSet.Include(d => d.Laboratories).ToListAsync();
}
