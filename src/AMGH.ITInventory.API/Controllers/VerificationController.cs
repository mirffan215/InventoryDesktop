using AMGH.ITInventory.Application.Services;
using AMGH.ITInventory.Security;
using AMGH.ITInventory.Shared.Contracts;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMGH.ITInventory.API.Controllers;

[ApiController, ApiVersion("1.0"), Route("api/v{version:apiVersion}/verification"), Authorize(Policy = Policies.RunVerification)]
public class VerificationController : ControllerBase
{
    private readonly IVerificationService _svc;
    public VerificationController(IVerificationService svc) => _svc = svc;

    [HttpGet("campaigns")]
    public async Task<ApiResponse<IReadOnlyList<CampaignDto>>> Campaigns(CancellationToken ct) => ApiResponse<IReadOnlyList<CampaignDto>>.Ok(await _svc.ListCampaignsAsync(ct));

    public record CreateCampaignRequest(string Name, DateTime StartDate, DateTime EndDate, int? DepartmentId, int? LocationId);

    [HttpPost("campaigns"), Authorize(Policy = Policies.ManageAssets)]
    public async Task<ActionResult<ApiResponse<int>>> Create(CreateCampaignRequest r, CancellationToken ct)
    {
        try { var c = await _svc.CreateCampaignAsync(r.Name, r.StartDate, r.EndDate, r.DepartmentId, r.LocationId, ct); return ApiResponse<int>.Ok(c.Id); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<int>.Fail(ex.Message)); }
    }

    /// <summary>Mobile / desktop offline sync push. Idempotent per ClientId.</summary>
    [HttpPost("sync")]
    public async Task<ApiResponse<SyncPushResult>> Sync(SyncPushRequest req, CancellationToken ct) =>
        ApiResponse<SyncPushResult>.Ok(await _svc.ApplySubmissionsAsync(req.DeviceId, req.Verifications, User.Identity?.Name ?? "unknown", ct));
}
