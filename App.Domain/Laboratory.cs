using Base.Domain;

namespace App.Domain;

public class Laboratory : BaseEntity
{
    public LangStr Name { get; set; } = default!;
    public LangStr? Description { get; set; }
    public int? MaxOccupancy { get; set; }

    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = default!;

    public Guid LocationId { get; set; }
    public Location Location { get; set; } = default!;

    public ICollection<Equipment>? Equipment { get; set; }
}
