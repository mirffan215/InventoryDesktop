using AMGH.ITInventory.Domain.Common;
using AMGH.ITInventory.Shared.Enums;

namespace AMGH.ITInventory.Domain.Entities;

public class VerificationCampaign : AuditableEntity
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public CampaignStatus Status { get; set; } = CampaignStatus.Draft;
    public int? DepartmentId { get; set; }
    public int? LocationId { get; set; }
    public ICollection<VerificationItem> Items { get; set; } = new List<VerificationItem>();
    public ICollection<VerificationApproval> Approvals { get; set; } = new List<VerificationApproval>();

    public void Activate()
    {
        if (Status != CampaignStatus.Draft) throw new InvalidOperationException("Only draft campaigns can be activated.");
        if (Items.Count == 0) throw new InvalidOperationException("Campaign has no assets to verify.");
        Status = CampaignStatus.Active;
    }

    public void SubmitForReview()
    {
        if (Status != CampaignStatus.Active) throw new InvalidOperationException("Only active campaigns can be submitted.");
        Status = CampaignStatus.UnderReview;
    }

    public void Complete()
    {
        if (Status != CampaignStatus.UnderReview) throw new InvalidOperationException("Campaign must be under review.");
        if (Approvals.Count == 0 || Approvals.Any(a => a.Status != ApprovalStatus.Approved))
            throw new InvalidOperationException("All approvals must be granted before completion.");
        Status = CampaignStatus.Completed;
    }
}

public class VerificationItem : AuditableEntity
{
    public int CampaignId { get; set; }
    public VerificationCampaign? Campaign { get; set; }
    public int AssetId { get; set; }
    public Asset? Asset { get; set; }
    public VerificationResult Result { get; set; } = VerificationResult.Pending;
    public VerificationMethod? Method { get; set; }
    public DateTime? VerifiedAtUtc { get; set; }
    public string? VerifiedBy { get; set; }
    public Guid? ClientId { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Comment { get; set; }
    public bool IsException => Result is not (VerificationResult.Pending or VerificationResult.Verified);
    public ICollection<VerificationEvidence> Evidence { get; set; } = new List<VerificationEvidence>();
}

public class VerificationEvidence : AuditableEntity
{
    public int VerificationItemId { get; set; }
    public string FileName { get; set; } = "";
    public string ContentType { get; set; } = "image/jpeg";
    public string StoragePath { get; set; } = "";
    public string Sha256 { get; set; } = "";
}

public class VerificationApproval : AuditableEntity
{
    public int CampaignId { get; set; }
    public string ApproverRole { get; set; } = "";
    public string? ApproverName { get; set; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    public DateTime? DecidedUtc { get; set; }
    public string? Comment { get; set; }
}
