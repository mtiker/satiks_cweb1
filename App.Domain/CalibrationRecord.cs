using System.ComponentModel.DataAnnotations;
using App.Domain.Identity;
using Base.Domain;

namespace App.Domain;

public class CalibrationRecord : BaseEntity
{
    public DateTime CalibrationDate { get; set; }
    public DateTime NextCalibrationDue { get; set; }
    [MaxLength(128)] 
    public string? CertificateNumber { get; set; }
    [MaxLength(1024)] 
    public string? Notes { get; set; }
    public bool Passed { get; set; } = true;

    public Guid EquipmentId { get; set; }
    public Equipment Equipment { get; set; } = default!;

    public Guid? CalibratedByUserId { get; set; }
    public AppUser? CalibratedByUser { get; set; }
}
