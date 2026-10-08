using AMGH.ITInventory.Application.Abstractions;
using AMGH.ITInventory.Domain.Entities;
using AMGH.ITInventory.Shared.Contracts;
using AMGH.ITInventory.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace AMGH.ITInventory.Application.Services;

public interface IVerificationService
{
    Task<VerificationCampaign> CreateCampaignAsync(string name, DateTime start, DateTime end, int? departmentId, int? locationId, CancellationToken ct = default);
    Task<SyncPushResult> ApplySubmissionsAsync(string deviceId, IEnumerable<VerificationSubmission> submissions, string user, CancellationToken ct = default);
    Task<IReadOnlyList<CampaignDto>> ListCampaignsAsync(CancellationToken ct = default);
}

public class VerificationService : IVerificationService
{
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;
    public VerificationService(IUnitOfWork uow, IClock clock) { _uow = uow; _clock = clock; }

    /// <summary>Creates a draft campaign and snapshots in-scope assets as pending items.</summary>
    public async Task<VerificationCampaign> CreateCampaignAsync(string name, DateTime start, DateTime end, int? departmentId, int? locationId, CancellationToken ct = default)
    {
        if (end < start) throw new ArgumentException("End date precedes start date.");
        var assets = await _uow.Repository<Asset>().Query()
            .Where(a => a.Status != AssetStatus.Disposed && a.Status != AssetStatus.Retired)
            .Where(a => departmentId == null || a.DepartmentId == departmentId)
            .Where(a => locationId == null || a.LocationId == locationId)
            .Select(a => a.Id).ToListAsync(ct);

        var campaign = new VerificationCampaign { Name = name, StartDate = start, EndDate = end, DepartmentId = departmentId, LocationId = locationId };
        foreach (var id in assets) campaign.Items.Add(new VerificationItem { AssetId = id });
        await _uow.Repository<VerificationCampaign>().AddAsync(campaign, ct);
        await _uow.SaveChangesAsync(ct);
        return campaign;
    }

    /// <summary>Idempotent: a ClientId already stored is ignored, so retried syncs never duplicate.</summary>
    public async Task<SyncPushResult> ApplySubmissionsAsync(string deviceId, IEnumerable<VerificationSubmission> submissions, string user, CancellationToken ct = default)
    {
        int accepted = 0, rejected = 0; var errors = new List<string>();
        foreach (var s in submissions)
        {
            var item = await _uow.Repository<VerificationItem>().Query()
                .Include(i => i.Asset).Include(i => i.Campaign)
                .FirstOrDefaultAsync(i => i.CampaignId == s.CampaignId && i.Asset!.AssetTag == s.AssetTag, ct);
            if (item is null) { rejected++; errors.Add($"{s.AssetTag}: not in campaign {s.CampaignId}"); continue; }
            if (item.Campaign!.Status != CampaignStatus.Active) { rejected++; errors.Add($"{s.AssetTag}: campaign not active"); continue; }
            if (item.ClientId == s.ClientId) { accepted++; continue; }

            item.Result = s.Result; item.Method = s.Method; item.ClientId = s.ClientId;
            item.VerifiedAtUtc = s.VerifiedAtUtc; item.VerifiedBy = user;
            item.Latitude = s.Latitude; item.Longitude = s.Longitude; item.Comment = s.Comment;
            if (s.Result == VerificationResult.Verified) item.Asset!.LastVerifiedUtc = s.VerifiedAtUtc;
            accepted++;
        }
        await _uow.SaveChangesAsync(ct);
        return new SyncPushResult(accepted, rejected, errors);
    }

    public async Task<IReadOnlyList<CampaignDto>> ListCampaignsAsync(CancellationToken ct = default) =>
        await _uow.Repository<VerificationCampaign>().Query().Select(c => new CampaignDto(c.Id, c.Name, c.StartDate, c.EndDate, c.Status,
            c.Items.Count, c.Items.Count(i => i.Result != VerificationResult.Pending))).ToListAsync(ct);
}
