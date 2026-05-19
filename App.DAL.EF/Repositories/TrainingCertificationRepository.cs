using App.DAL.Contracts;
using App.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.DAL.EF.Repositories;

public class TrainingCertificationRepository(AppDbContext context) : EFBaseRepository<TrainingCertification>(context), ITrainingCertificationRepository
{

    // appUserId matches domain field TrainingCertification.AppUserId — matches contract exactly
    public async Task<IEnumerable<TrainingCertification>> AllWithDetailsAsync(Guid? appUserId = null)
    {
        var q = RepositoryDbSet
            .Include(t => t.AppUser)
            .Include(t => t.EquipmentCategory)
            .Include(t => t.ValidatedByUser)
            .AsQueryable();
        if (appUserId.HasValue) q = q.Where(t => t.AppUserId == appUserId.Value);
        return await q.ToListAsync();
    }

    public async Task<TrainingCertification?> FindWithDetailsAsync(Guid id, Guid? appUserId = null)
    {
        var q = RepositoryDbSet
            .Include(t => t.AppUser)
            .Include(t => t.EquipmentCategory)
            .Include(t => t.ValidatedByUser)
            .Where(t => t.Id == id);
        if (appUserId.HasValue) q = q.Where(t => t.AppUserId == appUserId.Value);
        return await q.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TrainingCertification>> GetByStatusAsync(TrainingCertificationStatus status)
        => await RepositoryDbSet
            .Where(t => t.Status == status)
            .Include(t => t.AppUser)
            .Include(t => t.EquipmentCategory)
            .Include(t => t.ValidatedByUser)
            .ToListAsync();

    // IMPORTANT: checks Status == Approved AND expiry — Pending/Rejected/Revoked/Expired must NOT grant access
    // Domain has no IsActive property — Status enum is the sole source of truth
    public async Task<bool> UserHasApprovedCertificationAsync(Guid appUserId, Guid categoryId)
        => await RepositoryDbSet.AnyAsync(t =>
            t.AppUserId == appUserId &&
            t.EquipmentCategoryId == categoryId &&
            t.Status == TrainingCertificationStatus.Approved &&
            (t.ExpiryDate == null || t.ExpiryDate > DateTime.UtcNow));

    // Returns Approved certs whose ExpiryDate has passed — used by background expiry job
    public async Task<IEnumerable<TrainingCertification>> GetExpiredAsync(DateTime? asOf = null)
    {
        var cutoff = asOf ?? DateTime.UtcNow;
        return await RepositoryDbSet
            .Where(t => t.Status == TrainingCertificationStatus.Approved &&
                        t.ExpiryDate != null && t.ExpiryDate <= cutoff)
            .Include(t => t.AppUser)
            .Include(t => t.EquipmentCategory)
            .ToListAsync();
    }

    // IDOR override: enforce ownership before delete
    public override async Task<bool> RemoveAsync(Guid id, Guid? appUserId = null)
    {
        var q = RepositoryDbSet.Where(t => t.Id == id);
        if (appUserId.HasValue) q = q.Where(t => t.AppUserId == appUserId.Value);
        var entity = await q.FirstOrDefaultAsync();
        if (entity == null) return false;
        Remove(entity);
        return true;
    }
}
