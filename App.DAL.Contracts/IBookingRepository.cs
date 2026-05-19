using App.Domain;

namespace App.DAL.Contracts;

public interface IBookingRepository : IBaseRepository<Booking>
{
    // appUserId matches domain field Booking.AppUserId — avoids ambiguity with other user FK names
    Task<IEnumerable<Booking>> AllWithDetailsAsync(Guid? appUserId = null);
    Task<Booking?> FindWithDetailsAsync(Guid id, Guid? appUserId = null);

    /// <summary>
    /// Returns true if an overlapping confirmed/pending booking exists for the equipment
    /// in the given time window, optionally excluding one booking (for edit scenarios).
    /// </summary>
    Task<bool> HasConflictingBookingAsync(
        Guid equipmentId,
        DateTime start,
        DateTime end,
        Guid? excludeBookingId = null);

    /// <summary>Filter all bookings for an equipment by status — useful for schedule views.</summary>
    Task<IEnumerable<Booking>> GetByEquipmentAsync(Guid equipmentId, BookingStatus? status = null);
}
