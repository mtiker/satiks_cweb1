using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using App.Domain.Identity;
using Base.Domain;

namespace App.Domain;

public class MaintenanceRecord : BaseEntity
{
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    [MaxLength(1024)] 
    public string Description { get; set; } = default!;
    [MaxLength(1024)] 
    public string? Resolution { get; set; }
    [Column(TypeName = "decimal(18,2)")] 
    public decimal? Cost { get; set; }
    public bool IsScheduled { get; set; } = true;

    public Guid EquipmentId { get; set; }
    public Equipment Equipment { get; set; } = default!;

    public Guid? PerformedByUserId { get; set; }
    public AppUser? PerformedByUser { get; set; }
}
