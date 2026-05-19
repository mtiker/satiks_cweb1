using System.ComponentModel.DataAnnotations;

namespace WebApp.Areas.Admin.ViewModels.Manufacturer;

public class ManufacturerCreateEditViewModel
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(256)]
    [Display(Name = "Name (English)")]
    public string NameEn { get; set; } = default!;

    [MaxLength(256)]
    [Display(Name = "Nimi (Eesti)")]
    public string? NameEt { get; set; }

    [MaxLength(64)] public string? Country { get; set; }
    [MaxLength(256)] public string? Website { get; set; }
    [MaxLength(64)] public string? SupportPhone { get; set; }
    [MaxLength(256)] public string? SupportEmail { get; set; }
}
