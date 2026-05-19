using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Areas.Admin.ViewModels.Laboratory;

public class LaboratoryCreateEditViewModel
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(256)]
    [Display(Name = "Name (English)")]
    public string NameEn { get; set; } = default!;

    [MaxLength(256)]
    [Display(Name = "Nimi (Eesti)")]
    public string? NameEt { get; set; }

    [MaxLength(512)]
    [Display(Name = "Description (English)")]
    public string? DescriptionEn { get; set; }

    [MaxLength(512)]
    [Display(Name = "Kirjeldus (Eesti)")]
    public string? DescriptionEt { get; set; }

    public int? MaxOccupancy { get; set; }
    [Required] public Guid DepartmentId { get; set; }
    [Required] public Guid LocationId { get; set; }

    public SelectList? DepartmentSelectList { get; set; }
    public SelectList? LocationSelectList { get; set; }
}
