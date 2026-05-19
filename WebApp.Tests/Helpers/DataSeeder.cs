using App.DAL.EF;
using App.Domain;
using App.Domain.Identity;
using Base.Domain;

namespace WebApp.Tests.Helpers;

public static class DataSeeder
{
    /// <summary>
    /// Seeds a minimal but complete object graph and returns the IDs of
    /// created entities. The caller (CustomWebApplicationFactory) stores
    /// these as instance state — DataSeeder itself holds no static state.
    /// </summary>
    public static SeedResult SeedData(AppDbContext ctx)
    {
        var testUser = new AppUser
        {
            Id                 = Guid.NewGuid(),
            UserName           = "user@test.ee",
            Email              = "user@test.ee",
            FirstName          = "Test",
            LastName           = "User",
            NormalizedUserName = "USER@TEST.EE",
            NormalizedEmail    = "USER@TEST.EE",
            EmailConfirmed     = true,
            SecurityStamp      = Guid.NewGuid().ToString(),
            ConcurrencyStamp   = Guid.NewGuid().ToString(),
            PasswordHash       = "AQAAAAIAAYagAAAAE",
        };
        var secondUser = new AppUser
        {
            Id                 = Guid.NewGuid(),
            UserName           = "other@test.ee",
            Email              = "other@test.ee",
            FirstName          = "Other",
            LastName           = "User",
            NormalizedUserName = "OTHER@TEST.EE",
            NormalizedEmail    = "OTHER@TEST.EE",
            EmailConfirmed     = true,
            SecurityStamp      = Guid.NewGuid().ToString(),
            ConcurrencyStamp   = Guid.NewGuid().ToString(),
            PasswordHash       = "AQAAAAIAAYagAAAAE",
        };
        ctx.Users.Add(testUser);
        ctx.Users.Add(secondUser);

        var dept = new Department { Name = new LangStr("Test Dept",     "en") };
        var loc  = new Location   { BuildingName = new LangStr("Test Building", "en") };
        var lab  = new Laboratory { Name = "Test Lab", Department = dept, Location = loc };
        var cat  = new EquipmentCategory
            { Name = new LangStr("Test Category", "en"), RequiresTraining = false };
        var mfg  = new Manufacturer { Name = "Test Manufacturer" };

        ctx.Departments.Add(dept);
        ctx.Locations.Add(loc);
        ctx.Laboratories.Add(lab);
        ctx.EquipmentCategories.Add(cat);
        ctx.Manufacturers.Add(mfg);

        var equipment = new Equipment
        {
            Name                  = "Test Oscilloscope",
            Laboratory            = lab,
            EquipmentCategory     = cat,
            Manufacturer          = mfg,
            EquipmentCondition    = EquipmentCondition.Good,
            IsAvailableForBooking = true,
        };
        ctx.Equipment.Add(equipment);

        ctx.SaveChanges();

        // IDs are only valid after SaveChanges — return them to the caller.
        return new SeedResult(testUser.Id, secondUser.Id, equipment.Id, cat.Id);
    }
}
