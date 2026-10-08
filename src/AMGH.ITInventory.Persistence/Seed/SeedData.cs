using AMGH.ITInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AMGH.ITInventory.Persistence.Seed;

public static class SeedData
{
    public static async Task EnsureSeededAsync(AmghDbContext db, CancellationToken ct = default)
    {
        if (await db.AssetCategories.AnyAsync(ct)) return;
        db.AssetCategories.AddRange(
            new AssetCategory { Name = "Desktop", UsefulLifeMonths = 60 }, new AssetCategory { Name = "Laptop", UsefulLifeMonths = 48 },
            new AssetCategory { Name = "Server", UsefulLifeMonths = 72 }, new AssetCategory { Name = "Network Device", UsefulLifeMonths = 84 },
            new AssetCategory { Name = "Printer", UsefulLifeMonths = 60 }, new AssetCategory { Name = "Medical IT Device", UsefulLifeMonths = 96 },
            new AssetCategory { Name = "Mobile Device", UsefulLifeMonths = 36 });
        db.Departments.AddRange(
            new Department { Code = "IT", Name = "Information Technology" }, new Department { Code = "RAD", Name = "Radiology" },
            new Department { Code = "LAB", Name = "Laboratory" }, new Department { Code = "PHA", Name = "Pharmacy" },
            new Department { Code = "ER", Name = "Emergency" });
        db.Locations.Add(new Location { Building = "Main", Floor = "Ground", Room = "IT Store" });
        await db.SaveChangesAsync(ct);
    }
}
