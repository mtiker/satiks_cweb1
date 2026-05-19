using System.ComponentModel.DataAnnotations;

namespace App.DTO.v1;

public class BookingDto
{
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Purpose { get; set; }
    public string? Notes { get; set; }

    /// <summary>Valid values: Pending, Confirmed, Cancelled, Completed</summary>
    public string BookingStatus { get; set; } = default!;

    public Guid EquipmentId { get; set; }
    public string? EquipmentName { get; set; }
    public Guid AppUserId { get; set; }
    public string? UserFullName { get; set; }
}

public class BookingCreateDto
{
    [Required]
    public Guid EquipmentId { get; set; }

    /// <summary>Must be in UTC. Validation must enforce StartTime &lt; EndTime.</summary>
    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [MaxLength(512)]
    public string? Purpose { get; set; }

    [MaxLength(1024)]
    public string? Notes { get; set; }
}

public class BookingUpdateDto
{
    public Guid Id { get; set; }

    /// <summary>Must be in UTC. Validation must enforce StartTime &lt; EndTime.</summary>
    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [MaxLength(512)]
    public string? Purpose { get; set; }

    [MaxLength(1024)]
    public string? Notes { get; set; }

    /// <summary>Valid values: Pending, Confirmed, Cancelled, Completed</summary>
    public string BookingStatus { get; set; } = "Pending";
}
