using App.DAL.Contracts;
using App.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.DAL.EF.Repositories;

public class CalibrationRecordRepository(AppDbContext context) : EFBaseRepository<CalibrationRecord>(context), ICalibrationRecordRepository
{

    // calibratedByUserId matches domain field CalibrationRecord.CalibratedByUserId — matches contract exactly
    public async Task<IEnumerable<CalibrationRecord>> AllWithDetailsAsync(Guid? calibratedByUserId = null)
    {
        var q = RepositoryDbSet
            .Include(c => c.Equipment)
            .Include(c => c.CalibratedByUser)
            .AsQueryable();
        if (calibratedByUserId.HasValue) q = q.Where(c => c.CalibratedByUserId == calibratedByUserId.Value);
        return await q.OrderByDescending(c => c.CalibrationDate).ToListAsync();
    }

    public async Task<CalibrationRecord?> FindWithDetailsAsync(Guid id, Guid? calibratedByUserId = null)
    {
        var q = RepositoryDbSet
            .Include(c => c.Equipment)
            .Include(c => c.CalibratedByUser)
            .Where(c => c.Id == id);
        if (calibratedByUserId.HasValue) q = q.Where(c => c.CalibratedByUserId == calibratedByUserId.Value);
        return await q.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<CalibrationRecord>> GetByEquipmentAsync(Guid equipmentId)
        => await RepositoryDbSet
            .Where(c => c.EquipmentId == equipmentId)
            .Include(c => c.CalibratedByUser)
            .OrderByDescending(c => c.CalibrationDate)
            .ToListAsync();

    public async Task<CalibrationRecord?> GetLatestByEquipmentAsync(Guid equipmentId)
        => await RepositoryDbSet
            .Where(c => c.EquipmentId == equipmentId)
            .OrderByDescending(c => c.CalibrationDate)
            .FirstOrDefaultAsync();

    // Returns records where NextCalibrationDue <= asOf — used to alert admins or block bookings
    public async Task<IEnumerable<CalibrationRecord>> GetOverdueAsync(DateTime? asOf = null)
    {
        var cutoff = asOf ?? DateTime.UtcNow;
        return await RepositoryDbSet
            .Where(c => c.NextCalibrationDue <= cutoff)
            .Include(c => c.Equipment)
            .OrderBy(c => c.NextCalibrationDue)
            .ToListAsync();
    }
}
