using AMGH.ITInventory.Shared.Client;
using AMGH.ITInventory.Shared.Contracts;
using AMGH.ITInventory.Shared.Enums;
using Xunit;

namespace AMGH.ITInventory.Tests;

public class SyncEngineTests
{
    private class Store : IPendingVerificationStore
    {
        public List<VerificationSubmission> Pending = new(); public List<Guid> Synced = new();
        public Task<IReadOnlyList<VerificationSubmission>> GetPendingAsync(int max, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<VerificationSubmission>>(Pending.Where(p => !Synced.Contains(p.ClientId)).Take(max).ToList());
        public Task MarkSyncedAsync(IEnumerable<Guid> ids, CancellationToken ct = default) { Synced.AddRange(ids); return Task.CompletedTask; }
    }
    private class Api : IAmghApiClient
    {
        public bool Fail; public int Calls;
        public string? AccessToken { get; set; }
        public Task<ApiResponse<LoginResponse>?> LoginAsync(string u, string p, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<CampaignDto>> GetCampaignsAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public Task<AssetDto?> GetAssetByTagAsync(string t, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<PagedResult<AssetDto>> GetAssetsAsync(int p, int s, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<SyncPushResult> PushVerificationsAsync(SyncPushRequest r, CancellationToken ct = default)
        { Calls++; if (Fail) throw new HttpRequestException(); return Task.FromResult(new SyncPushResult(r.Verifications.Count, 0, Array.Empty<string>())); }
    }
    private static VerificationSubmission Sub(int i) => new(Guid.NewGuid(), 1, "PC-" + i, VerificationResult.Verified, VerificationMethod.Qr, DateTime.UtcNow, null, null, null, null);

    [Fact]
    public async Task Sends_in_batches_and_marks_synced()
    {
        var store = new Store { Pending = Enumerable.Range(1, 250).Select(Sub).ToList() }; var api = new Api();
        var o = await new SyncEngine(api, store, "dev").SyncAsync(100);
        Assert.Equal(250, o.Sent); Assert.Equal(3, api.Calls); Assert.Equal(250, store.Synced.Count); Assert.False(o.Offline);
    }

    [Fact]
    public async Task Offline_keeps_items_queued()
    {
        var store = new Store { Pending = Enumerable.Range(1, 5).Select(Sub).ToList() };
        var o = await new SyncEngine(new Api { Fail = true }, store, "dev").SyncAsync();
        Assert.True(o.Offline); Assert.Empty(store.Synced);
    }
}
