using App.DAL.Contracts;
using App.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.DAL.EF.Repositories;

public class LaboratoryRepository(AppDbContext context) : EFBaseRepository<Laboratory>(context), ILaboratoryRepository
{
    public async Task<IEnumerable<Laboratory>> AllWithDetailsAsync()
        => await RepositoryDbSet
            .Include(l => l.Department)
            .Include(l => l.Location)
            .Include(l => l.Equipment)
            .ToListAsync();

    public async Task<Laboratory?> FindWithDetailsAsync(Guid id)
        => await RepositoryDbSet
            .Include(l => l.Department)
            .Include(l => l.Location)
            .Include(l => l.Equipment)
            .FirstOrDefaultAsync(l => l.Id == id);
}
