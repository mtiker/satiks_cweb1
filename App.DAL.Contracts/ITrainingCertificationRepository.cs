using App.Domain;

namespace App.DAL.Contracts;

public interface ITrainingCertificationRepository : IBaseRepository<TrainingCertification>
{
    // appUserId matches domain field TrainingCertification.AppUserId
    Task<IEnumerable<TrainingCertification>> AllWithDetailsAsync(Guid? appUserId = null);
    Task<TrainingCertification?> FindWithDetailsAsync(Guid id, Guid? appUserId = null);

    /// <summary>
    /// Filter certifications by status — used by admin approval workflow.
    /// e.g. GetByStatusAsync(TrainingCertificationStatus.Pending)
    /// </summary>
    Task<IEnumerable<TrainingCertification>> GetByStatusAsync(TrainingCertificationStatus status);

    /// <summary>
    /// IMPORTANT: checks Status == Approved AND ExpiryDate is null or in the future.
    /// A Pending, Rejected, Revoked, or Expired certification must NOT grant access.
    /// Used by booking service to gate access to RequiresTraining equipment categories.
    /// </summary>
    Task<bool> UserHasApprovedCertificationAsync(Guid appUserId, Guid categoryId);

    /// <summary>
    /// Returns certifications that are Approved but have an ExpiryDate &lt;= asOf.
    /// Used by a background job or admin view to bulk-transition them to Expired.
    /// </summary>
    Task<IEnumerable<TrainingCertification>> GetExpiredAsync(DateTime? asOf = null);
}
