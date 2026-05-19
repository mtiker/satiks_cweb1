using App.Domain;
using App.DTO.v1;

namespace App.BLL.Mappers;

// Domain has NO IsActive property — Status (enum) is the sole source of truth.
// Two update paths: user edits own pending submission; admin changes status.
public static class TrainingCertificationBllDtoFactory
{
    public static TrainingCertificationDto CreateDto(TrainingCertification t) => new()
    {
        Id = t.Id,
        CertifiedDate = t.CertifiedDate, ExpiryDate = t.ExpiryDate,
        CertificateReference = t.CertificateReference,
        Status = t.Status.ToString(),                   // enum → string; valid values documented in DTO
        AppUserId = t.AppUserId,
        AppUserFullName = t.AppUser?.FullName,
        EquipmentCategoryId = t.EquipmentCategoryId,
        EquipmentCategoryName = t.EquipmentCategory?.Name.ToString(),
        ValidatedByUserId = t.ValidatedByUserId,        // Guid? — nullable
        ValidatedByUserName = t.ValidatedByUser?.FullName,
        ValidatedAt = t.ValidatedAt,
        ValidationComment = t.ValidationComment,
    };

    // User submits a new certification — Status always starts as Pending (never from DTO)
    public static TrainingCertification CreateEntity(TrainingCertificationCreateDto dto, Guid appUserId) => new()
    {
        CertifiedDate = dto.CertifiedDate, ExpiryDate = dto.ExpiryDate,
        CertificateReference = dto.CertificateReference,
        EquipmentCategoryId = dto.EquipmentCategoryId,
        AppUserId = appUserId,                          // from identity claim, never from DTO
        Status = TrainingCertificationStatus.Pending,   // always Pending on creation
    };

    // User edits their own pending submission — cannot change Status
    public static void UpdateEntity(TrainingCertification entity, TrainingCertificationUserUpdateDto dto)
    {
        entity.CertifiedDate = dto.CertifiedDate;
        entity.ExpiryDate = dto.ExpiryDate;
        entity.CertificateReference = dto.CertificateReference;
    }

    // Admin approves/rejects/revokes — ValidatedByUserId and ValidatedAt set server-side
    public static void UpdateEntityStatus(
        TrainingCertification entity,
        TrainingCertificationAdminUpdateDto dto,
        Guid adminUserId)
    {
        entity.Status = Enum.Parse<TrainingCertificationStatus>(dto.Status);
        entity.ValidationComment = dto.ValidationComment;
        entity.ValidatedByUserId = adminUserId;         // from admin's identity claim, never from DTO
        entity.ValidatedAt = DateTime.UtcNow;           // server-side timestamp, never from DTO
    }
}
