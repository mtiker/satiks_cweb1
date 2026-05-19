using System.ComponentModel.DataAnnotations;

namespace App.DTO.v1;

public class DepartmentDto
{
    public Guid Id { get; set; }

    /// <summary>Mapped from LangStr via culture resolution.</summary>
    public string Name { get; set; } = default!;

    public string? NameEn { get; set; }
    public string? NameEt { get; set; }

    [MaxLength(512)]
    public string? Description { get; set; }

    [MaxLength(64)]
    public string? CostCenter { get; set; }

    /// <summary>Read-only. Denormalized count of laboratories.</summary>
    public int LaboratoryCount { get; set; }
}

public class DepartmentCreateDto
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

    [MaxLength(64)]
    public string? CostCenter { get; set; }
}

public class DepartmentUpdateDto : DepartmentCreateDto
{
    public Guid Id { get; set; }
}