using App.Domain;
using App.Domain.Identity;
using Base.Domain;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace App.DAL.EF;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>, IDataProtectionKeyContext
{
    public DbSet<Department> Departments { get; set; } = default!;
    public DbSet<Location> Locations { get; set; } = default!;
    public DbSet<Laboratory> Laboratories { get; set; } = default!;
    public DbSet<EquipmentCategory> EquipmentCategories { get; set; } = default!;
    public DbSet<Manufacturer> Manufacturers { get; set; } = default!;
    public DbSet<Equipment> Equipment { get; set; } = default!;
    public DbSet<Booking> Bookings { get; set; } = default!;
    public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; } = default!;
    public DbSet<CalibrationRecord> CalibrationRecords { get; set; } = default!;
    public DbSet<TrainingCertification> TrainingCertifications { get; set; } = default!;
    public DbSet<AppRefreshToken> RefreshTokens { get; set; } = default!;
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = default!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Reusable options instance — avoids repeated null casts across LangStr conversions
    private static readonly JsonSerializerOptions JsonOptions = new();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Disable cascade delete globally — prevents accidental cascades
        foreach (var relationship in builder.Model.GetEntityTypes()
                     .SelectMany(e => e.GetForeignKeys()))
            relationship.DeleteBehavior = DeleteBehavior.Restrict;

        // LangStr — stored as jsonb in PostgreSQL
        // Provide a ValueComparer so EF can detect changes correctly for LangStr
        var langStrComparer = new ValueComparer<LangStr>(
            (l1, l2) => JsonSerializer.Serialize(l1, JsonOptions) == JsonSerializer.Serialize(l2, JsonOptions),
            l => l == null ? 0 : JsonSerializer.Serialize(l, JsonOptions).GetHashCode(),
            l => JsonSerializer.Deserialize<LangStr>(JsonSerializer.Serialize(l, JsonOptions), JsonOptions)!
        );

        var nullableLangStrComparer = new ValueComparer<LangStr?>(
            (l1, l2) => (l1 == null && l2 == null) ||
                        (l1 != null && l2 != null &&
                         JsonSerializer.Serialize(l1, JsonOptions) == JsonSerializer.Serialize(l2, JsonOptions)),
            l => l == null ? 0 : JsonSerializer.Serialize(l, JsonOptions).GetHashCode(),
            l => l == null ? null : JsonSerializer.Deserialize<LangStr>(JsonSerializer.Serialize(l, JsonOptions), JsonOptions)
        );

        // --- Department ---
        builder.Entity<Department>().Property(e => e.Name)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<LangStr>(v, JsonOptions)!)
            .HasColumnType("jsonb");
        builder.Entity<Department>().Property(e => e.Name).Metadata.SetValueComparer(langStrComparer);

        // --- Location ---
        builder.Entity<Location>().Property(e => e.BuildingName)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<LangStr>(v, JsonOptions)!)
            .HasColumnType("jsonb");
        builder.Entity<Location>().Property(e => e.BuildingName).Metadata.SetValueComparer(langStrComparer);

        // --- EquipmentCategory ---
        builder.Entity<EquipmentCategory>().Property(e => e.Name)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<LangStr>(v, JsonOptions)!)
            .HasColumnType("jsonb");
        builder.Entity<EquipmentCategory>().Property(e => e.Name).Metadata.SetValueComparer(langStrComparer);

        // --- Equipment ---
        builder.Entity<Equipment>().Property(e => e.Name)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<LangStr>(v, JsonOptions)!)
            .HasColumnType("jsonb");
        builder.Entity<Equipment>().Property(e => e.Name).Metadata.SetValueComparer(langStrComparer);

        builder.Entity<Equipment>().Property(e => e.Description)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, JsonOptions),
                v => v == null ? null : JsonSerializer.Deserialize<LangStr>(v, JsonOptions))
            .HasColumnType("jsonb");
        builder.Entity<Equipment>().Property(e => e.Description).Metadata.SetValueComparer(nullableLangStrComparer);

        // --- Laboratory ---
        builder.Entity<Laboratory>().Property(e => e.Name)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<LangStr>(v, JsonOptions)!)
            .HasColumnType("jsonb");
        builder.Entity<Laboratory>().Property(e => e.Name).Metadata.SetValueComparer(langStrComparer);

        builder.Entity<Laboratory>().Property(e => e.Description)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, JsonOptions),
                v => v == null ? null : JsonSerializer.Deserialize<LangStr>(v, JsonOptions))
            .HasColumnType("jsonb");
        builder.Entity<Laboratory>().Property(e => e.Description).Metadata.SetValueComparer(nullableLangStrComparer);

        // --- Manufacturer ---
        builder.Entity<Manufacturer>().Property(e => e.Name)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<LangStr>(v, JsonOptions)!)
            .HasColumnType("jsonb");
        builder.Entity<Manufacturer>().Property(e => e.Name).Metadata.SetValueComparer(langStrComparer);

        // AppUser → Department (optional FK — allow SetNull on user delete)
        builder.Entity<AppUser>()
            .HasOne(u => u.Department).WithMany(d => d.Users)
            .HasForeignKey(u => u.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull).IsRequired(false);

        // MaintenanceRecord → AppUser (optional FK)
        builder.Entity<MaintenanceRecord>()
            .HasOne(m => m.PerformedByUser).WithMany(u => u.MaintenanceRecords)
            .HasForeignKey(m => m.PerformedByUserId)
            .OnDelete(DeleteBehavior.SetNull).IsRequired(false);

        // CalibrationRecord → AppUser (optional FK)
        builder.Entity<CalibrationRecord>()
            .HasOne(c => c.CalibratedByUser).WithMany(u => u.CalibrationRecords)
            .HasForeignKey(c => c.CalibratedByUserId)
            .OnDelete(DeleteBehavior.SetNull).IsRequired(false);

        // TrainingCertification has TWO FKs to AppUser — EF cannot auto-resolve; must be explicit.
        // AppUserId — the certified user (required)
        builder.Entity<TrainingCertification>()
            .HasOne(t => t.AppUser).WithMany(u => u.TrainingCertifications)
            .HasForeignKey(t => t.AppUserId)
            .OnDelete(DeleteBehavior.Restrict).IsRequired();

        // ValidatedByUserId — the admin who approved/rejected (optional)
        builder.Entity<TrainingCertification>()
            .HasOne(t => t.ValidatedByUser).WithMany()
            .HasForeignKey(t => t.ValidatedByUserId)
            .OnDelete(DeleteBehavior.SetNull).IsRequired(false);
    }
}
