using System.ComponentModel.DataAnnotations;
using App.Domain.Identity;
using Base.Domain;

namespace App.Domain;

public class Booking : BaseEntity
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    [MaxLength(512)] 
    public string? Purpose { get; set; }
    [MaxLength(1024)] 
    public string? Notes { get; set; }
    public BookingStatus BookingStatus { get; set; } = BookingStatus.Pending;

    public Guid EquipmentId { get; set; }
    public Equipment Equipment { get; set; } = default!;

    public Guid AppUserId { get; set; }
    public AppUser AppUser { get; set; } = default!;
}
