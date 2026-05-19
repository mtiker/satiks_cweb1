using App.Domain;

namespace App.DAL.Contracts;

public interface IMaintenanceRecordRepository : IBaseRepository<MaintenanceRecord>
{
    // performedByUserId matches domain field MaintenanceRecord.PerformedByUserId (nullable)
    Task<IEnumerable<MaintenanceRecord>> AllWithDetailsAsync(Guid? performedByUserId = null);
    Task<MaintenanceRecord?> FindWithDetailsAsync(Guid id, Guid? performedByUserId = null);

    Task<IEnumerable<MaintenanceRecord>> GetByEquipmentAsync(Guid equipmentId);

    /// <summary>
    /// Returns scheduled (IsScheduled == true) and not yet completed (CompletedDate == null) records.
    /// Used for maintenance calendar / upcoming tasks view.
    /// </summary>
    Task<IEnumerable<MaintenanceRecord>> GetScheduledAsync();
}
