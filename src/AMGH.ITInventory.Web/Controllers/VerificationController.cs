using AMGH.ITInventory.Application.Services;
using AMGH.ITInventory.Domain.Entities;
using AMGH.ITInventory.Persistence;
using AMGH.ITInventory.Reporting.Reports;
using AMGH.ITInventory.Shared.Enums;
using AMGH.ITInventory.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AMGH.ITInventory.Web.Controllers;

public class VerificationController : Controller
{
    private readonly AmghDbContext _db; private readonly IVerificationService _svc; private readonly IReportGenerator _reports;
    public VerificationController(AmghDbContext db, IVerificationService svc, IReportGenerator reports) { _db = db; _svc = svc; _reports = reports; }

    public async Task<IActionResult> Index(CancellationToken ct) => View(await _svc.ListCampaignsAsync(ct));

    [Authorize(Policy = "ManageAssets")]
    public IActionResult Create() => View(new CampaignFormViewModel());

    [HttpPost, Authorize(Policy = "ManageAssets")]
    public async Task<IActionResult> Create(CampaignFormViewModel vm, CancellationToken ct)
    {
        if (vm.EndDate < vm.StartDate) ModelState.AddModelError(nameof(vm.EndDate), "End date must be after start date");
        if (!ModelState.IsValid) return View(vm);
        var c = await _svc.CreateCampaignAsync(vm.Name, vm.StartDate, vm.EndDate, vm.DepartmentId, vm.LocationId, ct);
        return RedirectToAction(nameof(Details), new { id = c.Id });
    }

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var c = await _db.VerificationCampaigns.AsNoTracking().Include(x => x.Approvals).Include(x => x.Items).ThenInclude(i => i.Asset).FirstOrDefaultAsync(x => x.Id == id, ct);
        return c is null ? NotFound() : View(c);
    }

    [HttpPost, Authorize(Policy = "ManageAssets")]
    public async Task<IActionResult> Activate(int id, CancellationToken ct)
    {
        var c = await _db.VerificationCampaigns.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (c is null) return NotFound();
        try { c.Activate(); await _db.SaveChangesAsync(ct); TempData["Success"] = "Campaign activated."; }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Policy = "ViewReports")]
    public async Task<IActionResult> Report(int id, CancellationToken ct)
    {
        var c = await _db.VerificationCampaigns.AsNoTracking().Include(x => x.Items).ThenInclude(i => i.Asset).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (c is null) return NotFound();
        var table = new ReportTable($"Verification - {c.Name}", new[] { "Asset Tag", "Name", "Result", "Verified By", "Verified (UTC)", "Comment" },
            c.Items.Select(i => new[] { i.Asset!.AssetTag, i.Asset.Name, i.Result.ToString(), i.VerifiedBy ?? "", i.VerifiedAtUtc?.ToString("u") ?? "", i.Comment ?? "" }).ToList());
        return File(_reports.ToExcel(table), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"verification-{id}.xlsx");
    }
}
