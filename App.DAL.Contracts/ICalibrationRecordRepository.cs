using App.Domain;

namespace App.DAL.Contracts;

public interface ICalibrationRecordRepository : IBaseRepository<CalibrationRecord>
{
    // calibratedByUserId matches domain field CalibrationRecord.CalibratedByUserId (nullable)
    Task<IEnumerable<CalibrationRecord>> AllWithDetailsAsync(Guid? calibratedByUserId = null);
    Task<CalibrationRecord?> FindWithDetailsAsync(Guid id, Guid? calibratedByUserId = null);

    Task<IEnumerable<CalibrationRecord>> GetByEquipmentAsync(Guid equipmentId);

    /// <summary>Most recent record for an equipment — used to display calibration status.</summary>
    Task<CalibrationRecord?> GetLatestByEquipmentAsync(Guid equipmentId);

    /// <summary>
    /// Records where NextCalibrationDue &lt;= asOf (defaults to UtcNow).
    /// Used to alert admins / block bookings on uncalibrated equipment.
    /// </summary>
    Task<IEnumerable<CalibrationRecord>> GetOverdueAsync(DateTime? asOf = null);
}
