using AMGH.ITInventory.Domain.Common;
using AMGH.ITInventory.Shared.Enums;

namespace AMGH.ITInventory.Domain.Entities;

public class AssetCategory : AuditableEntity
{
    public string Name { get; set; } = "";
    public int UsefulLifeMonths { get; set; } = 60;
}

public class Asset : AuditableEntity
{
    public string AssetTag { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public int CategoryId { get; set; }
    public AssetCategory? Category { get; set; }
    public int? LocationId { get; set; }
    public Location? Location { get; set; }
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }
    /// <summary>ISO 27001 A.5.9/A.5.10 – every asset has a named owner.</summary>
    public int? OwnerEmployeeId { get; set; }
    public Employee? Owner { get; set; }
    public int? CustodianEmployeeId { get; set; }
    public Employee? Custodian { get; set; }
    public AssetStatus Status { get; set; } = AssetStatus.InStock;
    public AssetCondition Condition { get; set; } = AssetCondition.New;
    public SecurityClassification Classification { get; set; } = SecurityClassification.Internal;
    public int? VendorId { get; set; }
    public Vendor? Vendor { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchaseCost { get; set; }
    public DateTime? WarrantyEnd { get; set; }
    public string? IntuneDeviceId { get; set; }
    public string? IpAddress { get; set; }
    public string? MacAddress { get; set; }
    public DateTime? LastVerifiedUtc { get; set; }
    public ICollection<AssetMovement> Movements { get; set; } = new List<AssetMovement>();
    public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();

    public bool IsWarrantyExpired(DateTime utcNow) => WarrantyEnd.HasValue && WarrantyEnd.Value < utcNow;
}

public class AssetMovement : AuditableEntity
{
    public int AssetId { get; set; }
    public Asset? Asset { get; set; }
    public MovementType Type { get; set; }
    public int? FromEmployeeId { get; set; }
    public int? ToEmployeeId { get; set; }
    public int? FromLocationId { get; set; }
    public int? ToLocationId { get; set; }
    public string? Notes { get; set; }
}

public class MaintenanceRecord : AuditableEntity
{
    public int AssetId { get; set; }
    public Asset? Asset { get; set; }
    public MaintenanceType Type { get; set; }
    public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Scheduled;
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public decimal? Cost { get; set; }
    public string? Description { get; set; }
}

public class SoftwareLicense : AuditableEntity
{
    public string Product { get; set; } = "";
    public string? LicenseKeyHash { get; set; }
    public int TotalSeats { get; set; }
    public int UsedSeats { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? VendorId { get; set; }
}

public class AuditLog : Entity
{
    public DateTime TimestampUtc { get; set; }
    public string? UserName { get; set; }
    public string EntityName { get; set; } = "";
    public string EntityKey { get; set; } = "";
    public string Action { get; set; } = "";
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
}
