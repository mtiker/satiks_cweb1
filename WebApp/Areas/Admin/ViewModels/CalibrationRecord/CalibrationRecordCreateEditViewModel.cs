using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Areas.Admin.ViewModels.CalibrationRecord;

public class CalibrationRecordCreateEditViewModel
{
    public Guid Id { get; set; }
    [Required] public DateTime CalibrationDate { get; set; }
    [Required] public DateTime NextCalibrationDue { get; set; }
    [MaxLength(256)] public string? CertificateNumber { get; set; }
    [MaxLength(1024)] public string? Notes { get; set; }
    public bool Passed { get; set; }
    [Required] public Guid EquipmentId { get; set; }
    public Guid? CalibratedByUserId { get; set; }

    public SelectList? EquipmentSelectList { get; set; }
}