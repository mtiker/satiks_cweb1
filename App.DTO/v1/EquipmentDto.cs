using System.ComponentModel.DataAnnotations;

namespace App.DTO.v1;

public class EquipmentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? NameEn { get; set; }
    public string? NameEt { get; set; }
    public string? SerialNumber { get; set; }
    public string? ModelName { get; set; }
    public string? Description { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionEt { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? WarrantyExpiry { get; set; }
    public decimal? PurchasePrice { get; set; }
    public bool IsAvailableForBooking { get; set; }

    /// <summary>Valid values: New, Good, Fair, NeedsRepair, Decommissioned</summary>
    public string EquipmentCondition { get; set; } = default!;

    public Guid LaboratoryId { get; set; }
    public string? LaboratoryName { get; set; }
    public Guid EquipmentCategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Guid ManufacturerId { get; set; }
    public string? ManufacturerName { get; set; }

    /// <summary>Read-only. Denormalized from EquipmentCategory.RequiresTraining. Never included in write DTOs.</summary>
    public bool RequiresTraining { get; set; }
}

public class EquipmentCreateDto
{
    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = default!;

    [MaxLength(256)]
    public string? NameEt { get; set; }

    [MaxLength(128)]
    public string? SerialNumber { get; set; }

    [MaxLength(256)]
    public string? ModelName { get; set; }

    [MaxLength(1024)]
    public string? Description { get; set; }

    [MaxLength(1024)]
    public string? DescriptionEt { get; set; }

    public DateTime? PurchaseDate { get; set; }
    public DateTime? WarrantyExpiry { get; set; }
    public decimal? PurchasePrice { get; set; }
    public bool IsAvailableForBooking { get; set; } = true;

    /// <summary>Valid values: New, Good, Fair, NeedsRepair, Decommissioned</summary>
    public string EquipmentCondition { get; set; } = "Good";

    [Required]
    public Guid LaboratoryId { get; set; }

    [Required]
    public Guid EquipmentCategoryId { get; set; }

    [Required]
    public Guid ManufacturerId { get; set; }
}

public class EquipmentUpdateDto : EquipmentCreateDto
{
    public Guid Id { get; set; }
}
