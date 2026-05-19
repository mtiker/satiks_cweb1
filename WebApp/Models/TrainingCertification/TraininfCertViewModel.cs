using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Models.TrainingCertification;

/// <summary>ViewModel for user-submitted certification (Create / Edit own Pending record)</summary>
public class TrainingCertFormViewModel
{
    public Guid Id { get; set; }           // populated for Edit, empty for Create

    [Required]
    [Display(Name = "CertifiedDate")]
    [DataType(DataType.Date)]
    public DateTime CertifiedDate { get; set; } = DateTime.Today;

    [Display(Name = "ExpiryDate")]
    [DataType(DataType.Date)]
    public DateTime? ExpiryDate { get; set; }

    [MaxLength(128)]
    [Display(Name = "CertificateReference")]
    public string? CertificateReference { get; set; }

    [Required]
    [Display(Name = "EquipmentCategory")]
    public Guid EquipmentCategoryId { get; set; }

    // For the category dropdown
    public SelectList? CategorySelectList { get; set; }
}