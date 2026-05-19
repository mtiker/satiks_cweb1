using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain;

public class Location : BaseEntity
{
    public LangStr BuildingName { get; set; } = default!;

    [MaxLength(16)] 
    public string? Floor { get; set; }

    [MaxLength(16)] 
    public string? RoomNumber { get; set; }
    
    [MaxLength(256)] 
    public string? Address { get; set; }

    public ICollection<Laboratory>? Laboratories { get; set; }
}
