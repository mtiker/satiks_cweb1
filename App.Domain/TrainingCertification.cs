using System.ComponentModel.DataAnnotations;
using App.Domain.Identity;
using Base.Domain;

namespace App.Domain;

public class TrainingCertification : BaseEntity
{
    public DateTime CertifiedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    [MaxLength(128)]
    public string? CertificateReference { get; set; }

    public TrainingCertificationStatus Status { get; set; } = TrainingCertificationStatus.Pending;

    public Guid AppUserId { get; set; }                   
    public AppUser AppUser { get; set; } = default!;

    public Guid EquipmentCategoryId { get; set; }
    public EquipmentCategory EquipmentCategory { get; set; } = default!;

    public Guid? ValidatedByUserId { get; set; }         
    public AppUser? ValidatedByUser { get; set; }

    public DateTime? ValidatedAt { get; set; }

    [MaxLength(1024)]
    public string? ValidationComment { get; set; }       
}