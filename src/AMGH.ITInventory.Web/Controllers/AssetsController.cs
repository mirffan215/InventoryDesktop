using AMGH.ITInventory.Domain.Entities;
using AMGH.ITInventory.Persistence;
using AMGH.ITInventory.QRCode;
using AMGH.ITInventory.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AMGH.ITInventory.Web.Controllers;

public class AssetsController : Controller
{
    private readonly AmghDbContext _db; private readonly IQrService _qr;
    public AssetsController(AmghDbContext db, IQrService qr) { _db = db; _qr = qr; }

    public async Task<IActionResult> Index(CancellationToken ct) =>
        View(await _db.Assets.AsNoTracking().Include(a => a.Category).Include(a => a.Location).Include(a => a.Custodian).OrderBy(a => a.AssetTag).Take(5000).ToListAsync(ct));

    [Authorize(Policy = "ManageAssets")]
    public async Task<IActionResult> Create(CancellationToken ct) { await LoadLookups(ct); return View("Form", new AssetFormViewModel()); }

    [HttpPost, Authorize(Policy = "ManageAssets")]
    public async Task<IActionResult> Create(AssetFormViewModel vm, CancellationToken ct)
    {
        if (!_qr.TryParseAssetPayload(vm.AssetTag, out var tag)) ModelState.AddModelError(nameof(vm.AssetTag), "Invalid asset tag");
        else if (await _db.Assets.AnyAsync(a => a.AssetTag == tag, ct)) ModelState.AddModelError(nameof(vm.AssetTag), "Asset tag already exists");
        if (!ModelState.IsValid) { await LoadLookups(ct); return View("Form", vm); }
        _db.Assets.Add(new Asset { AssetTag = tag, Name = vm.Name, CategoryId = vm.CategoryId, SerialNumber = vm.SerialNumber, LocationId = vm.LocationId,
            DepartmentId = vm.DepartmentId, Classification = vm.Classification, Status = vm.Status, WarrantyEnd = vm.WarrantyEnd });
        await _db.SaveChangesAsync(ct);
        TempData["Success"] = $"Asset {tag} created.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Qr(string id) => _qr.TryParseAssetPayload(id, out var t) ? File(_qr.GenerateQrPng(_qr.BuildAssetPayload(t)), "image/png") : BadRequest();

    private async Task LoadLookups(CancellationToken ct)
    {
        ViewBag.Categories = new SelectList(await _db.AssetCategories.OrderBy(c => c.Name).ToListAsync(ct), "Id", "Name");
        ViewBag.Departments = new SelectList(await _db.Departments.OrderBy(c => c.Name).ToListAsync(ct), "Id", "Name");
        ViewBag.Locations = new SelectList((await _db.Locations.ToListAsync(ct)).Select(l => new { l.Id, Name = l.FullName }), "Id", "Name");
    }
}
