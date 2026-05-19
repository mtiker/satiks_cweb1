using System.ComponentModel.DataAnnotations;

namespace WebApp.Areas.Admin.ViewModels.Location;

public class LocationCreateEditViewModel
{
    public Guid Id { get; set; }

    [Required][MaxLength(256)]
    [Display(Name = "Building Name (English)")]
    public string BuildingNameEn { get; set; } = default!;

    [MaxLength(256)]
    [Display(Name = "Building Name (Eesti)")]
    public string? BuildingNameEt { get; set; }

    [MaxLength(16)] public string? Floor { get; set; }
    [MaxLength(16)] public string? RoomNumber { get; set; }
    [MaxLength(256)] public string? Address { get; set; }
}