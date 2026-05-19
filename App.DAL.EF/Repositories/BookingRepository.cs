using App.DAL.Contracts;
using App.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.DAL.EF.Repositories;

public class BookingRepository(AppDbContext context) : EFBaseRepository<Booking>(context), IBookingRepository
{

    // appUserId matches domain field Booking.AppUserId — matches contract parameter name exactly
    public async Task<IEnumerable<Booking>> AllWithDetailsAsync(Guid? appUserId = null)
    {
        var q = RepositoryDbSet
            .Include(b => b.Equipment).ThenInclude(e => e.EquipmentCategory)
            .Include(b => b.Equipment).ThenInclude(e => e.Laboratory)
            .Include(b => b.AppUser)
            .AsQueryable();
        if (appUserId.HasValue) q = q.Where(b => b.AppUserId == appUserId.Value);
        return await q.OrderByDescending(b => b.StartTime).ToListAsync();
    }

    public async Task<Booking?> FindWithDetailsAsync(Guid id, Guid? appUserId = null)
    {
        var q = RepositoryDbSet
            .Include(b => b.Equipment).ThenInclude(e => e.EquipmentCategory)
            .Include(b => b.Equipment).ThenInclude(e => e.Laboratory)
            .Include(b => b.AppUser)
            .Where(b => b.Id == id);
        if (appUserId.HasValue) q = q.Where(b => b.AppUserId == appUserId.Value);
        return await q.FirstOrDefaultAsync();
    }

    public async Task<bool> HasConflictingBookingAsync(Guid equipmentId, DateTime start,
        DateTime end, Guid? excludeBookingId = null)
    {
        var q = RepositoryDbSet.Where(b =>
            b.EquipmentId == equipmentId &&
            b.BookingStatus != BookingStatus.Cancelled &&
            b.StartTime < end && b.EndTime > start);
        if (excludeBookingId.HasValue) q = q.Where(b => b.Id != excludeBookingId.Value);
        return await q.AnyAsync();
    }

    public async Task<IEnumerable<Booking>> GetByEquipmentAsync(Guid equipmentId, BookingStatus? status = null)
    {
        var q = RepositoryDbSet.Where(b => b.EquipmentId == equipmentId).AsQueryable();
        if (status.HasValue) q = q.Where(b => b.BookingStatus == status.Value);
        return await q.OrderByDescending(b => b.StartTime).ToListAsync();
    }

    // IDOR override: enforce ownership before delete
    public override async Task<bool> RemoveAsync(Guid id, Guid? appUserId = null)
    {
        var q = RepositoryDbSet.Where(b => b.Id == id);
        if (appUserId.HasValue) q = q.Where(b => b.AppUserId == appUserId.Value);
        var entity = await q.FirstOrDefaultAsync();
        if (entity == null) return false;
        Remove(entity);
        return true;
    }
}
