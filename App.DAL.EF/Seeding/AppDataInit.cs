using App.Domain;
using App.Domain.Identity;
using Base.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace App.DAL.EF.Seeding;

public static class AppDataInit
{
    public static void DeleteDatabase(AppDbContext context) => context.Database.EnsureDeleted();
    public static void MigrateDatabase(AppDbContext context) => context.Database.Migrate();

    public static void SeedIdentity(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        foreach (var roleName in InitialData.Roles)
        {
            if (roleManager.FindByNameAsync(roleName).Result != null) continue;
            var result = roleManager.CreateAsync(new AppRole { Name = roleName }).Result;
            if (!result.Succeeded) throw new ApplicationException($"Role '{roleName}' creation failed!");
        }

        foreach (var (email, password, firstName, lastName, roles) in InitialData.Users)
        {
            var user = userManager.FindByEmailAsync(email).Result;
            if (user == null)
            {
                user = new AppUser
                {
                    Email = email, UserName = email,
                    FirstName = firstName, LastName = lastName,
                    EmailConfirmed = true
                };
                var result = userManager.CreateAsync(user, password).Result;
                if (!result.Succeeded) throw new ApplicationException($"User '{email}' creation failed!");
            }
            foreach (var role in roles)
                if (!userManager.IsInRoleAsync(user, role).Result)
                    userManager.AddToRoleAsync(user, role).Wait();
        }
    }

    public static void SeedAppData(AppDbContext ctx)
    {
        if (ctx.Departments.Any()) return;

        // --- Reference data ---
        var dept = new Department { Name = new LangStr("Electronics", "en") };
        dept.Name.SetTranslation("Elektroonika", "et");

        var loc = new Location
        {
            BuildingName = new LangStr("Main Building", "en"),
            Floor = "2", RoomNumber = "201",
            Address = "Akadeemia tee 15, Tallinn"
        };
        loc.BuildingName.SetTranslation("Peahoone", "et");

        var labName = new LangStr("Electronics Lab A", "en");
        labName.SetTranslation("Elektroonika labor A", "et");
        var lab = new Laboratory
        {
            Name = labName,
            Department = dept,
            Location = loc,
            MaxOccupancy = 20
        };

        var cat = new EquipmentCategory
        {
            Name = new LangStr("Oscilloscopes", "en"),
            RequiresTraining = true,
            Description = "Signal visualisation instruments"
        };
        cat.Name.SetTranslation("Ostsilloskoobid", "et");

        var catBasic = new EquipmentCategory
        {
            Name = new LangStr("Multimeters", "en"),
            RequiresTraining = false,
            Description = "Basic measurement instruments"
        };
        catBasic.Name.SetTranslation("Multimeetrid", "et");

        var mfgName = new LangStr("Keysight Technologies", "en");
        mfgName.SetTranslation("Keysight Technologies", "et");
        var mfg = new Manufacturer
        {
            Name = mfgName, Country = "USA",
            Website = "https://keysight.com", SupportEmail = "support@keysight.com"
        };

        // --- Equipment ---
        var eq1Name = new LangStr("Oscilloscope DSO-X 1204G", "en");
        eq1Name.SetTranslation("Ostsilloskoob DSO-X 1204G", "et");
        var eq1 = new Equipment
        {
            Name = eq1Name,
            SerialNumber = "MY12345678",
            ModelName = "DSO-X 1204G",
            Laboratory = lab,
            EquipmentCategory = cat,
            Manufacturer = mfg,
            EquipmentCondition = EquipmentCondition.Good,
            IsAvailableForBooking = true,
            PurchaseDate = new DateTime(2022, 3, 15),
            WarrantyExpiry = new DateTime(2025, 3, 15),
            PurchasePrice = 3200.00m
        };

        var eq2Name = new LangStr("Digital Multimeter 34461A", "en");
        eq2Name.SetTranslation("Digitaalne multimeeter 34461A", "et");
        var eq2 = new Equipment
        {
            Name = eq2Name,
            SerialNumber = "MY98765432",
            ModelName = "34461A",
            Laboratory = lab,
            EquipmentCategory = catBasic,
            Manufacturer = mfg,
            EquipmentCondition = EquipmentCondition.Good,
            IsAvailableForBooking = true,
            PurchaseDate = new DateTime(2023, 1, 10),
            PurchasePrice = 850.00m
        };

        var eq3Name = new LangStr("Signal Generator 33600A", "en");
        eq3Name.SetTranslation("Signaalgeneraator 33600A", "et");
        var eq3 = new Equipment
        {
            Name = eq3Name,
            SerialNumber = "MY11112222",
            ModelName = "33600A",
            Laboratory = lab,
            EquipmentCategory = cat,
            Manufacturer = mfg,
            EquipmentCondition = EquipmentCondition.NeedsRepair,
            IsAvailableForBooking = false,
            PurchaseDate = new DateTime(2020, 6, 1)
        };

        ctx.Departments.Add(dept);
        ctx.Locations.Add(loc);
        ctx.Laboratories.Add(lab);
        ctx.EquipmentCategories.AddRange(cat, catBasic);
        ctx.Manufacturers.Add(mfg);
        ctx.Equipment.AddRange(eq1, eq2, eq3);
        ctx.SaveChanges();

        // --- Maintenance records (exercises GetScheduledAsync and GetByEquipmentAsync) ---
        ctx.MaintenanceRecords.AddRange(
            new MaintenanceRecord
            {
                Equipment = eq3, Description = "Power supply failure — awaiting parts",
                ScheduledDate = DateTime.UtcNow.AddDays(3),
                IsScheduled = true, CompletedDate = null
            },
            new MaintenanceRecord
            {
                Equipment = eq1, Description = "Annual preventive inspection",
                ScheduledDate = DateTime.UtcNow.AddDays(14),
                IsScheduled = true, CompletedDate = null
            }
        );

        // --- Calibration records (exercises GetLatestByEquipmentAsync and GetOverdueAsync) ---
        ctx.CalibrationRecords.AddRange(
            new CalibrationRecord
            {
                Equipment = eq1,
                CalibrationDate = DateTime.UtcNow.AddMonths(-6),
                NextCalibrationDue = DateTime.UtcNow.AddMonths(6),
                CertificateNumber = "CAL-2024-001", Passed = true
            },
            new CalibrationRecord
            {
                Equipment = eq2,
                CalibrationDate = DateTime.UtcNow.AddMonths(-14),
                NextCalibrationDue = DateTime.UtcNow.AddMonths(-2), // overdue — exercises GetOverdueAsync
                CertificateNumber = "CAL-2023-042", Passed = true
            }
        );

        ctx.SaveChanges();
    }
}
