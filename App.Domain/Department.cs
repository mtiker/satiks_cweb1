using System.ComponentModel.DataAnnotations;
using App.Domain.Identity;
using Base.Domain;

namespace App.Domain;

public class Department : BaseEntity
{
    public LangStr Name { get; set; } = default!;
    
    [MaxLength(512)] 
    public string? Description { get; set; }

    [MaxLength(64)] 
    public string? CostCenter { get; set; }

    public ICollection<Laboratory>? Laboratories { get; set; }
    public ICollection<AppUser>? Users { get; set; }
}
