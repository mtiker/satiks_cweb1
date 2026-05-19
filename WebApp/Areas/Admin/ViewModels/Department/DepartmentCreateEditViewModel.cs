using System.ComponentModel.DataAnnotations;

namespace WebApp.Areas.Admin.ViewModels.Department;

public class DepartmentCreateEditViewModel
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(256)]
    [Display(Name = "Name (English)")]
    public string NameEn { get; set; } = default!;

    [MaxLength(256)]
    [Display(Name = "Nimi (Eesti)")]
    public string? NameEt { get; set; }

    [MaxLength(512)] public string? Description { get; set; }
    [MaxLength(64)] public string? CostCenter { get; set; }
}