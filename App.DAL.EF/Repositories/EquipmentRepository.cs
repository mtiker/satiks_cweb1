using App.DAL.Contracts;
using App.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.DAL.EF.Repositories;

public class EquipmentRepository(AppDbContext context) : EFBaseRepository<Equipment>(context), IEquipmentRepository
{
    public async Task<IEnumerable<Equipment>> AllWithDetailsAsync()
        => await RepositoryDbSet
            .Include(e => e.Laboratory).ThenInclude(l => l.Department)
            .Include(e => e.Laboratory).ThenInclude(l => l.Location)
            .Include(e => e.EquipmentCategory)
            .Include(e => e.Manufacturer)
            .ToListAsync();

    public async Task<Equipment?> FindWithDetailsAsync(Guid id)
        => await RepositoryDbSet
            .Include(e => e.Laboratory).ThenInclude(l => l.Department)
            .Include(e => e.Laboratory).ThenInclude(l => l.Location)
            .Include(e => e.EquipmentCategory)
            .Include(e => e.Manufacturer)
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IEnumerable<Equipment>> GetAvailableEquipmentAsync()
        => await RepositoryDbSet
            .Where(e => e.IsAvailableForBooking &&
                        e.EquipmentCondition != EquipmentCondition.Decommissioned &&
                        e.EquipmentCondition != EquipmentCondition.NeedsRepair)
            .Include(e => e.Laboratory)
            .Include(e => e.EquipmentCategory)
            .Include(e => e.Manufacturer)
            .ToListAsync();

    public async Task<IEnumerable<Equipment>> GetByLaboratoryAsync(Guid laboratoryId)
        => await RepositoryDbSet
            .Where(e => e.LaboratoryId == laboratoryId)
            .Include(e => e.EquipmentCategory)
            .Include(e => e.Manufacturer)
            .ToListAsync();

    public async Task<IEnumerable<Equipment>> GetByCategoryAsync(Guid categoryId)
        => await RepositoryDbSet
            .Where(e => e.EquipmentCategoryId == categoryId)
            .Include(e => e.Laboratory)
            .Include(e => e.Manufacturer)
            .ToListAsync();

    // Implements IEquipmentRepository.GetByConditionAsync — leverages EquipmentCondition enum
    public async Task<IEnumerable<Equipment>> GetByConditionAsync(EquipmentCondition condition)
        => await RepositoryDbSet
            .Where(e => e.EquipmentCondition == condition)
            .Include(e => e.Laboratory)
            .Include(e => e.EquipmentCategory)
            .ToListAsync();
}
