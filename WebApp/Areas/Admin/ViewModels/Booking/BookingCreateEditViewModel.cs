using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Areas.Admin.ViewModels.Booking;

public class BookingCreateEditViewModel
{
    public Guid Id { get; set; }

    [Required]
    public Guid EquipmentId { get; set; }

    [Required]
    public Guid AppUserId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [MaxLength(512)]
    public string? Purpose { get; set; }

    public string? BookingStatus { get; set; }

    // Dropdowns
    public SelectList? EquipmentSelectList { get; set; }
    public SelectList? UserSelectList { get; set; }
    public SelectList? StatusSelectList { get; set; }
}