using System.ComponentModel.DataAnnotations;

namespace App.DTO.v1;

public class CalibrationRecordDto
{
    public Guid Id { get; set; }

    [Required]
    public DateTime CalibrationDate { get; set; }

    [Required]
    public DateTime NextCalibrationDue { get; set; }

    [MaxLength(128)]
    public string? CertificateNumber { get; set; }

    [MaxLength(1024)]
    public string? Notes { get; set; }

    public bool Passed { get; set; }

    public Guid EquipmentId { get; set; }
    public string? EquipmentName { get; set; }

    public Guid? CalibratedByUserId { get; set; }
    public string? CalibratedByUserName { get; set; }
}

public class CalibrationRecordCreateDto
{
    [Required]
    public DateTime CalibrationDate { get; set; }

    [Required]
    public DateTime NextCalibrationDue { get; set; }

    [MaxLength(128)]
    public string? CertificateNumber { get; set; }

    [MaxLength(1024)]
    public string? Notes { get; set; }

    public bool Passed { get; set; } = true;

    [Required]
    public Guid EquipmentId { get; set; }

    public Guid? CalibratedByUserId { get; set; }
}

public class CalibrationRecordUpdateDto : CalibrationRecordCreateDto
{
    public Guid Id { get; set; }
}
