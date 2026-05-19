using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain;

public class EquipmentCategory : BaseEntity
{
    public LangStr Name { get; set; } = default!;
    
    [MaxLength(512)] 
    public string? Description { get; set; }
    public bool RequiresTraining { get; set; }

    public ICollection<Equipment>? Equipment { get; set; }
    public ICollection<TrainingCertification>? TrainingCertifications { get; set; }
}
