using App.DTO.v1;
namespace App.BLL.Contracts;

public interface ITrainingCertificationService
{
    Task<IEnumerable<TrainingCertificationDto>> AllAsync(Guid? appUserId = null);
    Task<TrainingCertificationDto?> FindAsync(Guid id, Guid? appUserId = null);
    Task<TrainingCertificationDto> AddAsync(TrainingCertificationCreateDto dto, Guid appUserId);
    Task<TrainingCertificationDto> UpdateAsync(TrainingCertificationUserUpdateDto dto, Guid appUserId);
    Task<bool> RemoveAsync(Guid id, Guid? appUserId = null);

    // Admin approval workflow — maps to TrainingCertificationAdminUpdateDto from Prompt 5.
    // Sets Status, ValidationComment, ValidatedByUserId (from adminUserId), ValidatedAt (UtcNow) server-side.
    Task<TrainingCertificationDto> UpdateStatusAsync(TrainingCertificationAdminUpdateDto dto, Guid adminUserId);

    // Admin view — fetch all certs by status (e.g. Pending queue for approval)
    Task<IEnumerable<TrainingCertificationDto>> GetByStatusAsync(string status);

    // Booking gate — checks Status == Approved AND expiry. Named to match updated DAL contract.
    Task<bool> UserHasApprovedCertificationAsync(Guid appUserId, Guid categoryId);
}