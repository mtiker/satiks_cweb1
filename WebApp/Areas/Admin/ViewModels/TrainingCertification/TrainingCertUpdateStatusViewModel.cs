using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Areas.Admin.ViewModels.TrainingCertification;

public class TrainingCertUpdateStatusViewModel
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    [Display(Name = "Status")]
    public string Status { get; set; } = default!;

    [MaxLength(1024)]
    [Display(Name = "Validation Comment")]
    public string? ValidationComment { get; set; }

    public SelectList? StatusSelectList { get; set; }
}