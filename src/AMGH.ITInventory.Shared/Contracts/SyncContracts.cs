using AMGH.ITInventory.Shared.Enums;

namespace AMGH.ITInventory.Shared.Contracts;

public record LoginRequest(string UserName, string Password);
public record LoginResponse(string AccessToken, DateTime ExpiresUtc, string DisplayName, string[] Roles);

public record AssetDto(int Id, string AssetTag, string Name, string? SerialNumber, string Category,
    string? Location, string? Custodian, AssetStatus Status, SecurityClassification Classification, DateTime? WarrantyEnd);

/// <summary>A single verification captured offline on a device and pushed on sync.</summary>
public record VerificationSubmission(Guid ClientId, int CampaignId, string AssetTag, VerificationResult Result,
    VerificationMethod Method, DateTime VerifiedAtUtc, double? Latitude, double? Longitude, string? Comment, string? PhotoBase64);

public record SyncPushRequest(string DeviceId, IReadOnlyList<VerificationSubmission> Verifications);
public record SyncPushResult(int Accepted, int Rejected, IReadOnlyList<string> Errors);
public record CampaignDto(int Id, string Name, DateTime StartDate, DateTime EndDate, CampaignStatus Status, int TotalAssets, int VerifiedAssets);
