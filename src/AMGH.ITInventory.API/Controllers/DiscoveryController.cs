using AMGH.ITInventory.Infrastructure.Discovery;
using AMGH.ITInventory.Security;
using AMGH.ITInventory.Shared.Contracts;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMGH.ITInventory.API.Controllers;

[ApiController, ApiVersion("1.0"), Route("api/v{version:apiVersion}/discovery"), Authorize(Policy = Policies.ManageAssets)]
public class DiscoveryController : ControllerBase
{
    private readonly INetworkDiscoveryService _svc;
    public DiscoveryController(INetworkDiscoveryService svc) => _svc = svc;

    public record ScanRequest(string Cidr, string? SnmpCommunity);

    [HttpPost("scan")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DiscoveredDevice>>>> Scan(ScanRequest r, CancellationToken ct)
    {
        try { return ApiResponse<IReadOnlyList<DiscoveredDevice>>.Ok(await _svc.ScanAsync(r.Cidr, r.SnmpCommunity ?? "public", ct: ct)); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<IReadOnlyList<DiscoveredDevice>>.Fail(ex.Message)); }
    }
}
