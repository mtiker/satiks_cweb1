using System.ComponentModel.DataAnnotations;

namespace App.DTO.v1;

public class EquipmentCategoryDto
{
    public Guid Id { get; set; }

    /// <summary>Mapped from LangStr via culture resolution.</summary>
    public string Name { get; set; } = default!;

    public string? NameEn { get; set; }
    public string? NameEt { get; set; }

    [MaxLength(512)]
    public string? Description { get; set; }

    public bool RequiresTraining { get; set; }

    /// <summary>Read-only. Denormalized count of equipment.</summary>
    public int EquipmentCount { get; set; }
}

public class EquipmentCategoryCreateDto
{
    [Required]
    [MaxLength(256)]
    /// <summary>English name. Stored as the default culture translation.</summary>
    public string Name { get; set; } = default!;

    [MaxLength(256)]
    /// <summary>Optional Estonian translation. Stored as the 'et' culture slot.</summary>
    public string? NameEt { get; set; }

    [MaxLength(512)]
    public string? Description { get; set; }

    public bool RequiresTraining { get; set; }
}

public class EquipmentCategoryUpdateDto : EquipmentCategoryCreateDto
{
    public Guid Id { get; set; }
}