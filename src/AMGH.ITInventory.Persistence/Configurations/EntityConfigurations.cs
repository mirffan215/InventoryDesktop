using AMGH.ITInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AMGH.ITInventory.Persistence.Configurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> b)
    {
        b.ToTable("Assets");
        b.HasIndex(a => a.AssetTag).IsUnique().HasFilter("[IsDeleted] = 0");
        b.HasIndex(a => a.SerialNumber);
        b.HasIndex(a => new { a.Status, a.DepartmentId });
        b.Property(a => a.AssetTag).HasMaxLength(50).IsRequired();
        b.Property(a => a.Name).HasMaxLength(200).IsRequired();
        b.Property(a => a.SerialNumber).HasMaxLength(100);
        b.Property(a => a.IpAddress).HasMaxLength(45);
        b.Property(a => a.MacAddress).HasMaxLength(17);
        b.HasOne(a => a.Owner).WithMany().HasForeignKey(a => a.OwnerEmployeeId);
        b.HasOne(a => a.Custodian).WithMany().HasForeignKey(a => a.CustodianEmployeeId);
    }
}

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> b)
    {
        b.HasIndex(e => e.EmployeeNumber).IsUnique().HasFilter("[IsDeleted] = 0");
        b.HasIndex(e => e.EntraObjectId);
    }
}

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> b) => b.Ignore(l => l.FullName);
}

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> b) => b.HasIndex(d => d.Code).IsUnique().HasFilter("[IsDeleted] = 0");
}

public class VerificationItemConfiguration : IEntityTypeConfiguration<VerificationItem>
{
    public void Configure(EntityTypeBuilder<VerificationItem> b)
    {
        b.Ignore(i => i.IsException);
        b.HasIndex(i => new { i.CampaignId, i.AssetId }).IsUnique().HasFilter("[IsDeleted] = 0");
        b.HasIndex(i => i.ClientId);
    }
}

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> b)
    {
        b.Ignore(x => x.Remaining);
        b.HasIndex(x => new { x.FiscalYear, x.DepartmentId, x.Type }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public class PurchaseRequestConfiguration : IEntityTypeConfiguration<PurchaseRequest>
{
    public void Configure(EntityTypeBuilder<PurchaseRequest> b)
    {
        b.Ignore(x => x.Total);
        b.HasIndex(x => x.Number).IsUnique();
        b.HasMany(x => x.Lines).WithOne().HasForeignKey(l => l.PurchaseRequestId);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.HasIndex(a => new { a.EntityName, a.EntityKey });
        b.HasIndex(a => a.TimestampUtc);
    }
}
