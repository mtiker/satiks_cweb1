using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Areas.Admin.ViewModels.MaintenanceRecord;

public class MaintenanceRecordCreateEditViewModel
{
    public Guid Id { get; set; }
    [Required] public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    [Required][MaxLength(1024)] public string Description { get; set; } = default!;
    [MaxLength(1024)] public string? Resolution { get; set; }
    [Range(0, double.MaxValue)] public decimal? Cost { get; set; }
    public bool IsScheduled { get; set; }
    [Required] public Guid EquipmentId { get; set; }
    public Guid? PerformedByUserId { get; set; }

    public SelectList? EquipmentSelectList { get; set; }
}