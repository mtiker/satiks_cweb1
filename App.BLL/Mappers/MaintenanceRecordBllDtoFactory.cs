using App.Domain;
using App.DTO.v1;

namespace App.BLL.Mappers;

public static class MaintenanceRecordBllDtoFactory
{
    public static MaintenanceRecordDto CreateDto(MaintenanceRecord m) => new()
    {
        Id = m.Id,
        ScheduledDate = m.ScheduledDate, CompletedDate = m.CompletedDate,
        Description = m.Description, Resolution = m.Resolution,
        Cost = m.Cost, IsScheduled = m.IsScheduled,
        EquipmentId = m.EquipmentId, EquipmentName = m.Equipment?.Name,
        PerformedByUserId = m.PerformedByUserId,                    // Guid? — nullable in domain
        PerformedByUserName = m.PerformedByUser?.FullName,
    };

    public static MaintenanceRecord CreateEntity(MaintenanceRecordCreateDto dto) => new()
    {
        ScheduledDate = dto.ScheduledDate, CompletedDate = dto.CompletedDate,
        Description = dto.Description, Resolution = dto.Resolution,
        Cost = dto.Cost, IsScheduled = dto.IsScheduled,
        EquipmentId = dto.EquipmentId,
        PerformedByUserId = dto.PerformedByUserId,                  // Guid? — may be null
    };

    public static void UpdateEntity(MaintenanceRecord entity, MaintenanceRecordUpdateDto dto)
    {
        entity.ScheduledDate = dto.ScheduledDate; entity.CompletedDate = dto.CompletedDate;
        entity.Description = dto.Description; entity.Resolution = dto.Resolution;
        entity.Cost = dto.Cost; entity.IsScheduled = dto.IsScheduled;
        entity.PerformedByUserId = dto.PerformedByUserId;
    }
}
