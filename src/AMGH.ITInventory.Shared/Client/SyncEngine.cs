using AMGH.ITInventory.Shared.Contracts;

namespace AMGH.ITInventory.Shared.Client;

/// <summary>Device-local queue of verifications captured offline (SQLite on desktop and mobile).</summary>
public interface IPendingVerificationStore
{
    Task<IReadOnlyList<VerificationSubmission>> GetPendingAsync(int max, CancellationToken ct = default);
    Task MarkSyncedAsync(IEnumerable<Guid> clientIds, CancellationToken ct = default);
}

public record SyncOutcome(int Sent, int Accepted, int Rejected, IReadOnlyList<string> Errors, bool Offline);

/// <summary>
/// Pushes pending verifications in batches. Safe to retry: the server de-duplicates by ClientId,
/// so a batch is only marked synced after the server acknowledges it.
/// </summary>
public class SyncEngine
{
    private readonly IAmghApiClient _api; private readonly IPendingVerificationStore _store; private readonly string _deviceId;
    private readonly SemaphoreSlim _gate = new(1, 1);
    public SyncEngine(IAmghApiClient api, IPendingVerificationStore store, string deviceId) { _api = api; _store = store; _deviceId = deviceId; }

    public async Task<SyncOutcome> SyncAsync(int batchSize = 100, CancellationToken ct = default)
    {
        if (!await _gate.WaitAsync(0, ct)) return new SyncOutcome(0, 0, 0, new[] { "Sync already running" }, false);
        try
        {
            int sent = 0, accepted = 0, rejected = 0; var errors = new List<string>();
            while (true)
            {
                var batch = await _store.GetPendingAsync(batchSize, ct);
                if (batch.Count == 0) break;
                SyncPushResult result;
                try { result = await _api.PushVerificationsAsync(new SyncPushRequest(_deviceId, batch), ct); }
                catch (HttpRequestException) { return new SyncOutcome(sent, accepted, rejected, errors, Offline: true); }
                catch (TaskCanceledException) when (!ct.IsCancellationRequested) { return new SyncOutcome(sent, accepted, rejected, errors, Offline: true); }

                sent += batch.Count; accepted += result.Accepted; rejected += result.Rejected; errors.AddRange(result.Errors);
                // Server-side rejections (e.g. campaign closed) are permanent; mark them synced so the queue cannot jam.
                await _store.MarkSyncedAsync(batch.Select(b => b.ClientId), ct);
            }
            return new SyncOutcome(sent, accepted, rejected, errors, false);
        }
        finally { _gate.Release(); }
    }
}
