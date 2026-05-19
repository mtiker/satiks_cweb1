using App.DTO.v1;
namespace App.BLL.Contracts;

public interface IMaintenanceRecordService
{
    // Admin overloads — no ownership filter
    Task<IEnumerable<MaintenanceRecordDto>> AllAsync(Guid? performedByUserId = null);
    Task<MaintenanceRecordDto?> FindAsync(Guid id, Guid? performedByUserId = null);
    Task<MaintenanceRecordDto> AddAsync(MaintenanceRecordCreateDto dto);
    Task<MaintenanceRecordDto> UpdateAsync(MaintenanceRecordUpdateDto dto);
    Task<bool> RemoveAsync(Guid id);

    // User-scoped overloads — enforce ownership via PerformedByUserId
    Task<MaintenanceRecordDto> AddAsync(MaintenanceRecordCreateDto dto, Guid userId);
    Task<MaintenanceRecordDto> UpdateAsync(MaintenanceRecordUpdateDto dto, Guid userId);
    Task<bool> RemoveAsync(Guid id, Guid userId);

    Task<IEnumerable<MaintenanceRecordDto>> GetByEquipmentAsync(Guid equipmentId);
    // Returns IsScheduled == true and CompletedDate == null records — for maintenance calendar
    Task<IEnumerable<MaintenanceRecordDto>> GetScheduledAsync();
}
