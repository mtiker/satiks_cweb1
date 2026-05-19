using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain;

public class Manufacturer : BaseEntity
{
    public LangStr Name { get; set; } = default!;

    [MaxLength(64)]
    public string? Country { get; set; }

    [MaxLength(256)]
    public string? Website { get; set; }

    [MaxLength(64)]
    public string? SupportPhone { get; set; }

    [MaxLength(256)]
    public string? SupportEmail { get; set; }

    public ICollection<Equipment>? Equipment { get; set; }
}
