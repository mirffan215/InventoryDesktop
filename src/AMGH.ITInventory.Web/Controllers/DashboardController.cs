using AMGH.ITInventory.Persistence;
using AMGH.ITInventory.Shared.Enums;
using AMGH.ITInventory.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AMGH.ITInventory.Web.Controllers;

public class DashboardController : Controller
{
    private readonly AmghDbContext _db;
    public DashboardController(AmghDbContext db) => _db = db;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var soon = DateTime.UtcNow.AddDays(30);
        var byStatus = await _db.Assets.GroupBy(a => a.Status).Select(g => new { g.Key, N = g.Count() }).ToListAsync(ct);
        var byCat = await _db.Assets.GroupBy(a => a.Category!.Name).Select(g => new { g.Key, N = g.Count() }).OrderByDescending(x => x.N).ToListAsync(ct);
        int Count(AssetStatus s) => byStatus.FirstOrDefault(x => x.Key == s)?.N ?? 0;
        var vm = new DashboardViewModel(byStatus.Sum(x => x.N), Count(AssetStatus.InUse), Count(AssetStatus.InStock), Count(AssetStatus.InRepair),
            await _db.Assets.CountAsync(a => a.WarrantyEnd != null && a.WarrantyEnd >= DateTime.UtcNow && a.WarrantyEnd <= soon, ct),
            await _db.VerificationCampaigns.CountAsync(c => c.Status == CampaignStatus.Active, ct),
            byCat.Select(x => (x.Key, x.N)).ToList(), byStatus.Select(x => (x.Key.ToString(), x.N)).ToList());
        return View(vm);
    }
}
