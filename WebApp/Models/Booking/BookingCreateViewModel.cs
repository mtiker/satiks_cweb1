using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Models.Booking;

public class BookingCreateViewModel
{
    [Required]
    public Guid EquipmentId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [MaxLength(512)]
    public string? Purpose { get; set; }

    // For dropdown
    public SelectList? EquipmentSelectList { get; set; }
}