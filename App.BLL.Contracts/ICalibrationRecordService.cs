using App.DTO.v1;
namespace App.BLL.Contracts;

public interface ICalibrationRecordService
{
    // Admin overloads — no ownership filter
    Task<IEnumerable<CalibrationRecordDto>> AllAsync(Guid? calibratedByUserId = null);
    Task<CalibrationRecordDto?> FindAsync(Guid id, Guid? calibratedByUserId = null);
    Task<CalibrationRecordDto> AddAsync(CalibrationRecordCreateDto dto);
    Task<CalibrationRecordDto> UpdateAsync(CalibrationRecordUpdateDto dto);
    Task<bool> RemoveAsync(Guid id);

    // User-scoped overloads — enforce ownership via CalibratedByUserId
    Task<CalibrationRecordDto> AddAsync(CalibrationRecordCreateDto dto, Guid userId);
    Task<CalibrationRecordDto> UpdateAsync(CalibrationRecordUpdateDto dto, Guid userId);
    Task<bool> RemoveAsync(Guid id, Guid userId);

    Task<IEnumerable<CalibrationRecordDto>> GetByEquipmentAsync(Guid equipmentId);
    Task<CalibrationRecordDto?> GetLatestByEquipmentAsync(Guid equipmentId);
    // Returns records where NextCalibrationDue <= asOf — for admin alerts and booking gates
    Task<IEnumerable<CalibrationRecordDto>> GetOverdueAsync(DateTime? asOf = null);
}
