using System.ComponentModel.DataAnnotations;
using AMGH.ITInventory.Shared.Enums;

namespace AMGH.ITInventory.Web.ViewModels;

public record DashboardViewModel(int TotalAssets, int InUse, int InStock, int InRepair, int WarrantyExpiring30, int ActiveCampaigns,
    IReadOnlyList<(string Label, int Count)> ByCategory, IReadOnlyList<(string Label, int Count)> ByStatus);

public class AssetFormViewModel
{
    public int? Id { get; set; }
    [Required, StringLength(50), RegularExpression(@"^[A-Za-z0-9_-]+$", ErrorMessage = "Letters, digits, - and _ only")] public string AssetTag { get; set; } = "";
    [Required, StringLength(200)] public string Name { get; set; } = "";
    [Required] public int CategoryId { get; set; }
    [StringLength(100)] public string? SerialNumber { get; set; }
    public int? LocationId { get; set; }
    public int? DepartmentId { get; set; }
    public SecurityClassification Classification { get; set; } = SecurityClassification.Internal;
    public AssetStatus Status { get; set; } = AssetStatus.InStock;
    [DataType(DataType.Date)] public DateTime? WarrantyEnd { get; set; }
}

public class CampaignFormViewModel
{
    [Required, StringLength(200)] public string Name { get; set; } = "";
    [Required, DataType(DataType.Date)] public DateTime StartDate { get; set; } = DateTime.Today;
    [Required, DataType(DataType.Date)] public DateTime EndDate { get; set; } = DateTime.Today.AddDays(14);
    public int? DepartmentId { get; set; }
    public int? LocationId { get; set; }
}

public class DiscoveryViewModel { [Required] public string Cidr { get; set; } = "10.0.0.0/24"; public IReadOnlyList<AMGH.ITInventory.Infrastructure.Discovery.DiscoveredDevice>? Results { get; set; } }
