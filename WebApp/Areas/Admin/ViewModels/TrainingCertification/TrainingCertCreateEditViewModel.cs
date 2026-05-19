using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Areas.Admin.ViewModels.TrainingCertification;

public class TrainingCertCreateEditViewModel
{
    public Guid Id { get; set; }

    [Required]
    public Guid AppUserId { get; set; }

    [Required]
    public Guid EquipmentCategoryId { get; set; }

    [Required]
    public DateTime CertifiedDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    [MaxLength(128)]
    public string? CertificateReference { get; set; }

    // Dropdowns
    public SelectList? CategorySelectList { get; set; }
    public SelectList? UserSelectList { get; set; }
}