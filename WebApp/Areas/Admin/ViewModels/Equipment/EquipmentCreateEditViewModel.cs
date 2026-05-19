using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Areas.Admin.ViewModels.Equipment;

public class EquipmentCreateEditViewModel
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(256)]
    [Display(Name = "Name (English)")]
    public string NameEn { get; set; } = default!;

    [MaxLength(256)]
    [Display(Name = "Nimi (Eesti)")]
    public string? NameEt { get; set; }

    [MaxLength(128)] public string? SerialNumber { get; set; }
    [MaxLength(256)] public string? ModelName { get; set; }

    [MaxLength(1024)]
    [Display(Name = "Description (English)")]
    public string? DescriptionEn { get; set; }

    [MaxLength(1024)]
    [Display(Name = "Kirjeldus (Eesti)")]
    public string? DescriptionEt { get; set; }

    public DateTime? PurchaseDate { get; set; }
    public DateTime? WarrantyExpiry { get; set; }
    [Range(0, double.MaxValue)] public decimal? PurchasePrice { get; set; }
    public bool IsAvailableForBooking { get; set; } = true;
    [Required] public string EquipmentCondition { get; set; } = "Good";
    [Required] public Guid LaboratoryId { get; set; }
    [Required] public Guid EquipmentCategoryId { get; set; }
    [Required] public Guid ManufacturerId { get; set; }

    public SelectList? LaboratorySelectList { get; set; }
    public SelectList? CategorySelectList { get; set; }
    public SelectList? ManufacturerSelectList { get; set; }
    public SelectList? ConditionSelectList { get; set; }
}
