using System.Text.Json;
using AMGH.ITInventory.Application.Abstractions;
using AMGH.ITInventory.Domain.Common;
using AMGH.ITInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AMGH.ITInventory.Persistence;

public class AmghDbContext : DbContext
{
    private readonly ICurrentUser? _user;
    private readonly IClock? _clock;

    public AmghDbContext(DbContextOptions<AmghDbContext> options, ICurrentUser? user = null, IClock? clock = null) : base(options)
    { _user = user; _clock = clock; }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<AssetCategory> AssetCategories => Set<AssetCategory>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetMovement> AssetMovements => Set<AssetMovement>();
    public DbSet<MaintenanceRecord> MaintenanceRecords => Set<MaintenanceRecord>();
    public DbSet<SoftwareLicense> SoftwareLicenses => Set<SoftwareLicense>();
    public DbSet<VerificationCampaign> VerificationCampaigns => Set<VerificationCampaign>();
    public DbSet<VerificationItem> VerificationItems => Set<VerificationItem>();
    public DbSet<VerificationEvidence> VerificationEvidence => Set<VerificationEvidence>();
    public DbSet<VerificationApproval> VerificationApprovals => Set<VerificationApproval>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<PurchaseRequest> PurchaseRequests => Set<PurchaseRequest>();
    public DbSet<PurchaseRequestLine> PurchaseRequestLines => Set<PurchaseRequestLine>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.ApplyConfigurationsFromAssembly(typeof(AmghDbContext).Assembly);
        foreach (var et in b.Model.GetEntityTypes().Where(t => typeof(AuditableEntity).IsAssignableFrom(t.ClrType)))
        {
            var rv = b.Entity(et.ClrType).Property(nameof(AuditableEntity.RowVersion));
            if (Database.IsSqlServer()) rv.IsRowVersion(); else rv.IsConcurrencyToken();
            var p = System.Linq.Expressions.Expression.Parameter(et.ClrType, "e");
            var filter = System.Linq.Expressions.Expression.Lambda(
                System.Linq.Expressions.Expression.Not(System.Linq.Expressions.Expression.Property(p, nameof(AuditableEntity.IsDeleted))), p);
            b.Entity(et.ClrType).HasQueryFilter(filter);
        }
        foreach (var fk in b.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys())) fk.DeleteBehavior = DeleteBehavior.Restrict;
        foreach (var prop in b.Model.GetEntityTypes().SelectMany(t => t.GetProperties()).Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            { prop.SetPrecision(18); prop.SetScale(2); }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var now = _clock?.UtcNow ?? DateTime.UtcNow;
        var user = _user?.UserName ?? "system";
        var audits = new List<(AuditLog Log, Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Entry)>();

        foreach (var e in ChangeTracker.Entries().Where(e => e.Entity is not AuditLog && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToList())
        {
            if (e.Entity is AuditableEntity ae)
            {
                if (e.State == EntityState.Added) { ae.CreatedUtc = now; ae.CreatedBy = user; }
                else if (e.State == EntityState.Modified) { ae.ModifiedUtc = now; ae.ModifiedBy = user; }
                else if (e.State == EntityState.Deleted) { e.State = EntityState.Modified; ae.IsDeleted = true; ae.ModifiedUtc = now; ae.ModifiedBy = user; }
            }
            var props = e.Properties.Where(p => p.Metadata.Name != nameof(AuditableEntity.RowVersion));
            audits.Add((new AuditLog
            {
                TimestampUtc = now, UserName = user, IpAddress = _user?.IpAddress,
                EntityName = e.Metadata.ClrType.Name,
                EntityKey = "",
                Action = e.State == EntityState.Added ? "Create" : (e.Entity is AuditableEntity { IsDeleted: true } ? "Delete" : "Update"),
                OldValues = e.State == EntityState.Modified ? JsonSerializer.Serialize(props.Where(p => p.IsModified).ToDictionary(p => p.Metadata.Name, p => p.OriginalValue)) : null,
                NewValues = e.State != EntityState.Deleted ? JsonSerializer.Serialize(props.Where(p => e.State == EntityState.Added || p.IsModified).ToDictionary(p => p.Metadata.Name, p => p.CurrentValue)) : null,
            }, e));
        }
        var result = await base.SaveChangesAsync(ct);
        if (audits.Count > 0)
        {
            // Keys for inserts are only known after the first save, so resolve them now.
            foreach (var (log, entry) in audits)
                log.EntityKey = string.Join(",", entry.Metadata.FindPrimaryKey()!.Properties.Select(k => entry.Property(k.Name).CurrentValue));
            AuditLogs.AddRange(audits.Select(a => a.Log));
            await base.SaveChangesAsync(ct);
        }
        return result;
    }
}
