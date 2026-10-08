using AMGH.ITInventory.Application.Abstractions;
using AMGH.ITInventory.Domain.Entities;
using AMGH.ITInventory.QRCode;
using AMGH.ITInventory.Security;
using AMGH.ITInventory.Shared.Contracts;
using AMGH.ITInventory.Shared.Enums;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AMGH.ITInventory.API.Controllers;

[ApiController, ApiVersion("1.0"), Route("api/v{version:apiVersion}/assets"), Authorize]
public class AssetsController : ControllerBase
{
    private readonly IUnitOfWork _uow; private readonly IQrService _qr;
    public AssetsController(IUnitOfWork uow, IQrService qr) { _uow = uow; _qr = qr; }

    private static AssetDto ToDto(Asset a) => new(a.Id, a.AssetTag, a.Name, a.SerialNumber, a.Category!.Name,
        a.Location == null ? null : a.Location.Building + (a.Location.Room == null ? "" : " / " + a.Location.Room),
        a.Custodian?.DisplayName, a.Status, a.Classification, a.WarrantyEnd);

    [HttpGet]
    public async Task<ApiResponse<PagedResult<AssetDto>>> List([FromQuery] string? q, [FromQuery] AssetStatus? status, int page = 1, int pageSize = 50, CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 200); page = Math.Max(page, 1);
        var query = _uow.Repository<Asset>().Query().AsNoTracking().Include(a => a.Category).Include(a => a.Location).Include(a => a.Custodian).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(a => a.AssetTag.Contains(q) || a.Name.Contains(q) || (a.SerialNumber != null && a.SerialNumber.Contains(q)));
        if (status.HasValue) query = query.Where(a => a.Status == status);
        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(a => a.AssetTag).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return ApiResponse<PagedResult<AssetDto>>.Ok(new PagedResult<AssetDto> { Items = items.Select(ToDto).ToList(), Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("by-tag/{tag}")]
    public async Task<ActionResult<ApiResponse<AssetDto>>> ByTag(string tag, CancellationToken ct)
    {
        if (!_qr.TryParseAssetPayload(tag, out var parsed)) return BadRequest(ApiResponse<AssetDto>.Fail("Invalid asset tag"));
        var a = await _uow.Repository<Asset>().Query().AsNoTracking().Include(x => x.Category).Include(x => x.Location).Include(x => x.Custodian)
            .FirstOrDefaultAsync(x => x.AssetTag == parsed, ct);
        return a is null ? NotFound(ApiResponse<AssetDto>.Fail("Asset not found")) : ApiResponse<AssetDto>.Ok(ToDto(a));
    }

    [HttpGet("{tag}/qr"), Authorize(Policy = Policies.ManageAssets)]
    public IActionResult Qr(string tag) => _qr.TryParseAssetPayload(tag, out var t)
        ? File(_qr.GenerateQrPng(_qr.BuildAssetPayload(t)), "image/png") : BadRequest();

    public record CreateAssetRequest(string AssetTag, string Name, int CategoryId, string? SerialNumber, int? LocationId, int? DepartmentId,
        int? OwnerEmployeeId, SecurityClassification Classification, DateTime? WarrantyEnd);

    [HttpPost, Authorize(Policy = Policies.ManageAssets)]
    public async Task<ActionResult<ApiResponse<AssetDto>>> Create(CreateAssetRequest r, CancellationToken ct)
    {
        if (!_qr.TryParseAssetPayload(r.AssetTag, out var tag)) return BadRequest(ApiResponse<AssetDto>.Fail("Invalid asset tag"));
        if (await _uow.Repository<Asset>().Query().AnyAsync(a => a.AssetTag == tag, ct)) return Conflict(ApiResponse<AssetDto>.Fail("Asset tag already exists"));
        var asset = new Asset { AssetTag = tag, Name = r.Name, CategoryId = r.CategoryId, SerialNumber = r.SerialNumber, LocationId = r.LocationId,
            DepartmentId = r.DepartmentId, OwnerEmployeeId = r.OwnerEmployeeId, Classification = r.Classification, WarrantyEnd = r.WarrantyEnd };
        await _uow.Repository<Asset>().AddAsync(asset, ct);
        await _uow.SaveChangesAsync(ct);
        return await ByTag(tag, ct);
    }
}
