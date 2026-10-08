using AMGH.ITInventory.Shared.Client;
using AMGH.ITInventory.Shared.Contracts;
using AMGH.ITInventory.Shared.Enums;
using SQLite;

namespace AMGH.ITInventory.Mobile.Services;

public class PendingRow
{
    [PrimaryKey] public string ClientId { get; set; } = "";
    public int CampaignId { get; set; }
    public string AssetTag { get; set; } = "";
    public int Result { get; set; }
    public int Method { get; set; }
    public DateTime VerifiedAtUtc { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Comment { get; set; }
    public string? PhotoPath { get; set; }
    [Indexed] public bool Synced { get; set; }
}

public class AssetRow { [PrimaryKey] public string AssetTag { get; set; } = ""; public string Name { get; set; } = ""; public string? Location { get; set; } public string? Custodian { get; set; } }

public class LocalDatabase : IPendingVerificationStore
{
    private readonly SQLiteAsyncConnection _db;
    public LocalDatabase(string path)
    {
        _db = new SQLiteAsyncConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
        _db.CreateTableAsync<PendingRow>().Wait(); _db.CreateTableAsync<AssetRow>().Wait();
    }

    public Task CacheAssetsAsync(IEnumerable<AssetDto> a) =>
        _db.InsertAllAsync(a.Select(x => new AssetRow { AssetTag = x.AssetTag, Name = x.Name, Location = x.Location, Custodian = x.Custodian }), "OR REPLACE");
    public Task<AssetRow?> FindAssetAsync(string tag) => _db.Table<AssetRow>().Where(a => a.AssetTag == tag).FirstOrDefaultAsync()!;
    public Task EnqueueAsync(PendingRow r) => _db.InsertOrReplaceAsync(r);
    public Task<int> PendingCountAsync() => _db.Table<PendingRow>().Where(p => !p.Synced).CountAsync();

    public async Task<IReadOnlyList<VerificationSubmission>> GetPendingAsync(int max, CancellationToken ct = default)
    {
        var rows = await _db.Table<PendingRow>().Where(p => !p.Synced).Take(max).ToListAsync();
        return rows.Select(r => new VerificationSubmission(Guid.Parse(r.ClientId), r.CampaignId, r.AssetTag, (VerificationResult)r.Result, (VerificationMethod)r.Method,
            r.VerifiedAtUtc, r.Latitude, r.Longitude, r.Comment,
            r.PhotoPath is not null && File.Exists(r.PhotoPath) ? Convert.ToBase64String(File.ReadAllBytes(r.PhotoPath)) : null)).ToList();
    }

    public async Task MarkSyncedAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        foreach (var id in ids) await _db.ExecuteAsync("UPDATE PendingRow SET Synced = 1 WHERE ClientId = ?", id.ToString());
    }
}
