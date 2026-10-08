using AMGH.ITInventory.Application.Abstractions;
using AMGH.ITInventory.Application.Services;
using AMGH.ITInventory.Domain.Entities;
using AMGH.ITInventory.Infrastructure.Discovery;
using AMGH.ITInventory.Infrastructure.Services;
using AMGH.ITInventory.Persistence;
using AMGH.ITInventory.Persistence.Repositories;
using AMGH.ITInventory.QRCode;
using AMGH.ITInventory.Shared.Contracts;
using AMGH.ITInventory.Shared.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AMGH.ITInventory.Tests;

public class QrServiceTests
{
    private readonly QrService _qr = new();
    [Theory]
    [InlineData("AMGH-ASSET:pc-0001", "PC-0001")]
    [InlineData("pc-0001", "PC-0001")]
    public void Parses_valid_payloads(string input, string expected) { Assert.True(_qr.TryParseAssetPayload(input, out var tag)); Assert.Equal(expected, tag); }
    [Theory]
    [InlineData("")] [InlineData("a b")] [InlineData("x; DROP TABLE")] [InlineData("ab")]
    public void Rejects_invalid_payloads(string input) => Assert.False(_qr.TryParseAssetPayload(input, out _));
    [Fact] public void Generates_png() { var b = _qr.GenerateQrPng(_qr.BuildAssetPayload("PC-1")); Assert.Equal(0x89, b[0]); Assert.Equal((byte)'P', b[1]); }
}

public class DiscoveryTests
{
    [Fact] public void Expands_slash24_to_254_hosts() => Assert.Equal(254, NetworkDiscoveryService.ExpandCidr("10.1.2.0/24").Count());
    [Fact] public void Rejects_huge_ranges() => Assert.Throws<ArgumentException>(() => NetworkDiscoveryService.ExpandCidr("10.0.0.0/8").ToList());
    [Fact] public void Rejects_garbage() => Assert.Throws<ArgumentException>(() => NetworkDiscoveryService.ExpandCidr("nope").ToList());
    [Theory]
    [InlineData(new[] { 9100 }, null, "Printer")] [InlineData(new[] { 3389 }, null, "Windows Host")]
    [InlineData(new int[0], "Cisco IOS", "Network Device")] [InlineData(new int[0], null, "Unknown")]
    public void Fingerprints(int[] ports, string? snmp, string expected) => Assert.Equal(expected, NetworkDiscoveryService.Fingerprint(ports, snmp));
}

public class DomainTests
{
    [Fact]
    public void Campaign_cannot_complete_without_approval()
    {
        var c = new VerificationCampaign { Status = CampaignStatus.UnderReview };
        Assert.Throws<InvalidOperationException>(c.Complete);
        c.Approvals.Add(new VerificationApproval { Status = ApprovalStatus.Approved });
        c.Complete(); Assert.Equal(CampaignStatus.Completed, c.Status);
    }
    [Fact] public void Empty_campaign_cannot_activate() => Assert.Throws<InvalidOperationException>(new VerificationCampaign().Activate);
    [Fact]
    public void Budget_blocks_overcommit()
    {
        var b = new Budget { Allocated = 1000 }; b.Commit(600);
        Assert.Throws<InvalidOperationException>(() => b.Commit(401)); b.Commit(400); Assert.Equal(0, b.Remaining);
    }
}

public sealed class DbFixture : IDisposable
{
    private readonly SqliteConnection _conn = new("DataSource=:memory:");
    public DbFixture() { _conn.Open(); }
    public AmghDbContext NewContext()
    {
        var db = new AmghDbContext(new DbContextOptionsBuilder<AmghDbContext>().UseSqlite(_conn).Options, new FakeUser(), null);
        // SQLite has no rowversion generation; emulate it for tests.
        db.SavingChanges += (_, _) => { foreach (var e in db.ChangeTracker.Entries<AMGH.ITInventory.Domain.Common.AuditableEntity>().Where(e => e.State is EntityState.Added or EntityState.Modified)) e.Entity.RowVersion = Guid.NewGuid().ToByteArray(); };
        db.Database.EnsureCreated(); return db;
    }
    public void Dispose() => _conn.Dispose();
    private class FakeUser : ICurrentUser { public string? UserName => "tester"; public string? IpAddress => "127.0.0.1"; }
}

public class VerificationServiceTests : IClassFixture<DbFixture>
{
    private readonly DbFixture _fx; public VerificationServiceTests(DbFixture fx) => _fx = fx;

    [Fact]
    public async Task Sync_is_idempotent_and_records_audit()
    {
        using var db = _fx.NewContext();
        db.AssetCategories.Add(new AssetCategory { Name = "Laptop" }); await db.SaveChangesAsync();
        db.Assets.Add(new Asset { AssetTag = "PC-0001", Name = "Laptop", CategoryId = 1 }); await db.SaveChangesAsync();
        var svc = new VerificationService(new UnitOfWork(db), new SystemClock());
        var c = await svc.CreateCampaignAsync("Q4", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, null);
        Assert.Single(c.Items);
        var camp = await db.VerificationCampaigns.FindAsync(c.Id); camp!.Activate(); await db.SaveChangesAsync();

        var sub = new VerificationSubmission(Guid.NewGuid(), c.Id, "PC-0001", VerificationResult.Verified, VerificationMethod.Qr, DateTime.UtcNow, 1, 2, "ok", null);
        var r1 = await svc.ApplySubmissionsAsync("dev1", new[] { sub }, "alice");
        var r2 = await svc.ApplySubmissionsAsync("dev1", new[] { sub }, "alice");
        Assert.Equal(1, r1.Accepted); Assert.Equal(1, r2.Accepted); Assert.Equal(0, r2.Rejected);
        Assert.Equal(VerificationResult.Verified, (await db.VerificationItems.SingleAsync()).Result);
        Assert.NotNull((await db.Assets.SingleAsync()).LastVerifiedUtc);
        Assert.Contains(db.AuditLogs, a => a.EntityName == "Asset" && a.Action == "Update");
        Assert.DoesNotContain(db.AuditLogs, a => a.EntityKey == "");
    }

    [Fact]
    public async Task Sync_rejects_inactive_campaign_and_unknown_asset()
    {
        using var db = _fx.NewContext();
        db.AssetCategories.Add(new AssetCategory { Name = "X" }); await db.SaveChangesAsync();
        db.Assets.Add(new Asset { AssetTag = "PC-0002", Name = "L", CategoryId = db.AssetCategories.First().Id }); await db.SaveChangesAsync();
        var svc = new VerificationService(new UnitOfWork(db), new SystemClock());
        var c = await svc.CreateCampaignAsync("Draft one", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), null, null);
        var res = await svc.ApplySubmissionsAsync("d", new[]
        {
            new VerificationSubmission(Guid.NewGuid(), c.Id, "PC-0002", VerificationResult.Verified, VerificationMethod.Qr, DateTime.UtcNow, null, null, null, null),
            new VerificationSubmission(Guid.NewGuid(), c.Id, "NOPE-1", VerificationResult.Verified, VerificationMethod.Qr, DateTime.UtcNow, null, null, null, null)
        }, "bob");
        Assert.Equal(0, res.Accepted); Assert.Equal(2, res.Rejected);
    }

    [Fact]
    public async Task Soft_delete_hides_entity_but_keeps_row()
    {
        using var db = _fx.NewContext();
        var d = new Department { Code = "ZZ", Name = "Temp" }; db.Departments.Add(d); await db.SaveChangesAsync();
        db.Departments.Remove(d); await db.SaveChangesAsync();
        Assert.False(await db.Departments.AnyAsync(x => x.Code == "ZZ"));
        Assert.True(await db.Departments.IgnoreQueryFilters().AnyAsync(x => x.Code == "ZZ" && x.IsDeleted));
        Assert.Contains(db.AuditLogs, a => a.Action == "Delete");
    }
}
