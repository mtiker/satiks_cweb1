using Base.Contracts;
using Microsoft.AspNetCore.Identity;

namespace App.Domain.Identity;

public class AppUser : IdentityUser<Guid>, IBaseEntity
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string FullName => $"{FirstName} {LastName}";

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public ICollection<AppRefreshToken>? RefreshTokens { get; set; }
    public ICollection<Booking>? Bookings { get; set; }
    public ICollection<MaintenanceRecord>? MaintenanceRecords { get; set; }
    public ICollection<CalibrationRecord>? CalibrationRecords { get; set; }
    public ICollection<TrainingCertification>? TrainingCertifications { get; set; }
}
