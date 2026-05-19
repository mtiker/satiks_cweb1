using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Base.Domain;

namespace App.Domain;

public class Equipment : BaseEntity
{
    public LangStr Name { get; set; } = default!;
    [MaxLength(128)]
    public string? SerialNumber { get; set; }
    [MaxLength(256)]
    public string? ModelName { get; set; }
    public LangStr? Description { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? WarrantyExpiry { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal? PurchasePrice { get; set; }
    public bool IsAvailableForBooking { get; set; } = true;
    public EquipmentCondition EquipmentCondition { get; set; } = EquipmentCondition.Good;

    public Guid LaboratoryId { get; set; }
    public Laboratory Laboratory { get; set; } = default!;

    public Guid EquipmentCategoryId { get; set; }
    public EquipmentCategory EquipmentCategory { get; set; } = default!;

    public Guid ManufacturerId { get; set; }
    public Manufacturer Manufacturer { get; set; } = default!;

    public ICollection<Booking>? Bookings { get; set; }
    public ICollection<MaintenanceRecord>? MaintenanceRecords { get; set; }
    public ICollection<CalibrationRecord>? CalibrationRecords { get; set; }
}
