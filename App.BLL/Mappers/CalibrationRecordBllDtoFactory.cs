using App.Domain;
using App.DTO.v1;

namespace App.BLL.Mappers;

public static class CalibrationRecordBllDtoFactory
{
    public static CalibrationRecordDto CreateDto(CalibrationRecord c) => new()
    {
        Id = c.Id,
        CalibrationDate = c.CalibrationDate, NextCalibrationDue = c.NextCalibrationDue,
        CertificateNumber = c.CertificateNumber, Notes = c.Notes, Passed = c.Passed,
        EquipmentId = c.EquipmentId, EquipmentName = c.Equipment?.Name,
        CalibratedByUserId = c.CalibratedByUserId,                  // Guid? — nullable in domain
        CalibratedByUserName = c.CalibratedByUser?.FullName,
    };

    public static CalibrationRecord CreateEntity(CalibrationRecordCreateDto dto) => new()
    {
        CalibrationDate = dto.CalibrationDate, NextCalibrationDue = dto.NextCalibrationDue,
        CertificateNumber = dto.CertificateNumber, Notes = dto.Notes, Passed = dto.Passed,
        EquipmentId = dto.EquipmentId,
        CalibratedByUserId = dto.CalibratedByUserId,                // Guid? — may be null
    };

    public static void UpdateEntity(CalibrationRecord entity, CalibrationRecordUpdateDto dto)
    {
        entity.CalibrationDate = dto.CalibrationDate; entity.NextCalibrationDue = dto.NextCalibrationDue;
        entity.CertificateNumber = dto.CertificateNumber; entity.Notes = dto.Notes;
        entity.Passed = dto.Passed; entity.CalibratedByUserId = dto.CalibratedByUserId;
    }
}
