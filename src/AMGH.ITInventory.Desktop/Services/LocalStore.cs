using AMGH.ITInventory.Shared.Client;
using AMGH.ITInventory.Shared.Contracts;
using AMGH.ITInventory.Shared.Enums;
using Microsoft.Data.Sqlite;

namespace AMGH.ITInventory.Desktop.Services;

/// <summary>SQLite offline store: cached assets plus a queue of verifications awaiting sync.</summary>
public class LocalStore : IPendingVerificationStore
{
    private readonly string _cs;
    public LocalStore(string path)
    {
        _cs = $"Data Source={path}";
        Exec(@"CREATE TABLE IF NOT EXISTS Assets(AssetTag TEXT PRIMARY KEY, Json TEXT NOT NULL);
               CREATE TABLE IF NOT EXISTS Pending(ClientId TEXT PRIMARY KEY, CampaignId INTEGER, AssetTag TEXT, Result INTEGER, Method INTEGER,
                   VerifiedAtUtc TEXT, Lat REAL, Lon REAL, Comment TEXT, Synced INTEGER NOT NULL DEFAULT 0);");
    }

    private void Exec(string sql, params (string, object?)[] ps)
    {
        using var c = new SqliteConnection(_cs); c.Open(); using var cmd = c.CreateCommand(); cmd.CommandText = sql;
        foreach (var (n, v) in ps) cmd.Parameters.AddWithValue(n, v ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    public Task CacheAssetsAsync(IEnumerable<AssetDto> assets)
    {
        using var c = new SqliteConnection(_cs); c.Open(); using var tx = c.BeginTransaction();
        foreach (var a in assets)
        {
            using var cmd = c.CreateCommand(); cmd.Transaction = tx;
            cmd.CommandText = "INSERT OR REPLACE INTO Assets(AssetTag, Json) VALUES($t,$j)";
            cmd.Parameters.AddWithValue("$t", a.AssetTag); cmd.Parameters.AddWithValue("$j", System.Text.Json.JsonSerializer.Serialize(a)); cmd.ExecuteNonQuery();
        }
        tx.Commit(); return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AssetDto>> GetCachedAssetsAsync()
    {
        var list = new List<AssetDto>();
        using var c = new SqliteConnection(_cs); c.Open(); using var cmd = c.CreateCommand(); cmd.CommandText = "SELECT Json FROM Assets ORDER BY AssetTag";
        using var r = cmd.ExecuteReader(); while (r.Read()) list.Add(System.Text.Json.JsonSerializer.Deserialize<AssetDto>(r.GetString(0))!);
        return Task.FromResult<IReadOnlyList<AssetDto>>(list);
    }

    public void Enqueue(VerificationSubmission s) => Exec(
        "INSERT OR IGNORE INTO Pending(ClientId,CampaignId,AssetTag,Result,Method,VerifiedAtUtc,Lat,Lon,Comment) VALUES($id,$c,$t,$r,$m,$v,$la,$lo,$cm)",
        ("$id", s.ClientId.ToString()), ("$c", s.CampaignId), ("$t", s.AssetTag), ("$r", (int)s.Result), ("$m", (int)s.Method),
        ("$v", s.VerifiedAtUtc.ToString("O")), ("$la", s.Latitude), ("$lo", s.Longitude), ("$cm", s.Comment));

    public int PendingCount()
    {
        using var c = new SqliteConnection(_cs); c.Open(); using var cmd = c.CreateCommand(); cmd.CommandText = "SELECT COUNT(*) FROM Pending WHERE Synced=0";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public Task<IReadOnlyList<VerificationSubmission>> GetPendingAsync(int max, CancellationToken ct = default)
    {
        var list = new List<VerificationSubmission>();
        using var c = new SqliteConnection(_cs); c.Open(); using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT ClientId,CampaignId,AssetTag,Result,Method,VerifiedAtUtc,Lat,Lon,Comment FROM Pending WHERE Synced=0 LIMIT $m"; cmd.Parameters.AddWithValue("$m", max);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(new VerificationSubmission(Guid.Parse(r.GetString(0)), r.GetInt32(1), r.GetString(2), (VerificationResult)r.GetInt32(3), (VerificationMethod)r.GetInt32(4),
            DateTime.Parse(r.GetString(5), null, System.Globalization.DateTimeStyles.RoundtripKind), r.IsDBNull(6) ? null : r.GetDouble(6), r.IsDBNull(7) ? null : r.GetDouble(7), r.IsDBNull(8) ? null : r.GetString(8), null));
        return Task.FromResult<IReadOnlyList<VerificationSubmission>>(list);
    }

    public Task MarkSyncedAsync(IEnumerable<Guid> clientIds, CancellationToken ct = default)
    { foreach (var id in clientIds) Exec("UPDATE Pending SET Synced=1 WHERE ClientId=$id", ("$id", id.ToString())); return Task.CompletedTask; }
}
