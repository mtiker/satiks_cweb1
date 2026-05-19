using App.DAL.Contracts;
using App.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.DAL.EF.Repositories;

public class EquipmentCategoryRepository(AppDbContext context) : EFBaseRepository<EquipmentCategory>(context), IEquipmentCategoryRepository
{
    public async Task<IEnumerable<EquipmentCategory>> AllWithDetailsAsync()
        => await RepositoryDbSet.Include(c => c.Equipment).ToListAsync();

    // Returns only categories where RequiresTraining == true
    public async Task<IEnumerable<EquipmentCategory>> GetRequiringTrainingAsync()
        => await RepositoryDbSet
            .Where(c => c.RequiresTraining)
            .Include(c => c.Equipment)
            .ToListAsync();
}
