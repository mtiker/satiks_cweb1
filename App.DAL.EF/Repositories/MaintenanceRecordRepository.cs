using App.DAL.Contracts;
using App.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.DAL.EF.Repositories;

public class MaintenanceRecordRepository(AppDbContext context) : EFBaseRepository<MaintenanceRecord>(context), IMaintenanceRecordRepository
{

    // performedByUserId matches domain field MaintenanceRecord.PerformedByUserId — matches contract exactly
    public async Task<IEnumerable<MaintenanceRecord>> AllWithDetailsAsync(Guid? performedByUserId = null)
    {
        var q = RepositoryDbSet
            .Include(m => m.Equipment)
            .Include(m => m.PerformedByUser)
            .AsQueryable();
        if (performedByUserId.HasValue) q = q.Where(m => m.PerformedByUserId == performedByUserId.Value);
        return await q.OrderByDescending(m => m.ScheduledDate).ToListAsync();
    }

    public async Task<MaintenanceRecord?> FindWithDetailsAsync(Guid id, Guid? performedByUserId = null)
    {
        var q = RepositoryDbSet
            .Include(m => m.Equipment)
            .Include(m => m.PerformedByUser)
            .Where(m => m.Id == id);
        if (performedByUserId.HasValue) q = q.Where(m => m.PerformedByUserId == performedByUserId.Value);
        return await q.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<MaintenanceRecord>> GetByEquipmentAsync(Guid equipmentId)
        => await RepositoryDbSet
            .Where(m => m.EquipmentId == equipmentId)
            .Include(m => m.PerformedByUser)
            .OrderByDescending(m => m.ScheduledDate)
            .ToListAsync();

    // Returns IsScheduled == true AND CompletedDate == null — upcoming scheduled tasks
    public async Task<IEnumerable<MaintenanceRecord>> GetScheduledAsync()
        => await RepositoryDbSet
            .Where(m => m.IsScheduled && m.CompletedDate == null)
            .Include(m => m.Equipment)
            .Include(m => m.PerformedByUser)
            .OrderBy(m => m.ScheduledDate)
            .ToListAsync();
}
