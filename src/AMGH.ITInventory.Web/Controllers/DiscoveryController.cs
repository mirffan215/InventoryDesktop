using AMGH.ITInventory.Infrastructure.Discovery;
using AMGH.ITInventory.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMGH.ITInventory.Web.Controllers;

[Authorize(Policy = "ManageAssets")]
public class DiscoveryController : Controller
{
    private readonly INetworkDiscoveryService _svc;
    public DiscoveryController(INetworkDiscoveryService svc) => _svc = svc;
    public IActionResult Index() => View(new DiscoveryViewModel());

    [HttpPost]
    public async Task<IActionResult> Index(DiscoveryViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);
        try { vm.Results = await _svc.ScanAsync(vm.Cidr, ct: ct); }
        catch (ArgumentException ex) { ModelState.AddModelError(nameof(vm.Cidr), ex.Message); }
        return View(vm);
    }
}
