using System.Net.Http;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using AMGH.ITInventory.Desktop.Services;
using AMGH.ITInventory.Shared.Client;
using AMGH.ITInventory.Shared.Contracts;
using AMGH.ITInventory.Shared.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AMGH.ITInventory.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IAmghApiClient _api; private readonly LocalStore _store; private readonly SyncEngine _sync;

    [ObservableProperty] private string _userName = "";
    [ObservableProperty] private string _status = "Not signed in (offline mode)";
    [ObservableProperty] private string _scanInput = "";
    [ObservableProperty] private int _pendingCount;
    [ObservableProperty] private CampaignDto? _selectedCampaign;
    public ObservableCollection<AssetDto> Assets { get; } = new();
    public ObservableCollection<CampaignDto> Campaigns { get; } = new();

    public MainViewModel(IAmghApiClient api, LocalStore store, SyncEngine sync)
    {
        _api = api; _store = store; _sync = sync;
        _ = LoadCacheAsync();
    }

    private async Task LoadCacheAsync()
    {
        Assets.Clear(); foreach (var a in await _store.GetCachedAssetsAsync()) Assets.Add(a);
        PendingCount = _store.PendingCount();
    }

    [RelayCommand]
    private async Task LoginAsync(PasswordBox box)
    {
        try
        {
            var res = await _api.LoginAsync(UserName, box.Password);
            if (res?.Data is null) { Status = "Sign-in failed"; return; }
            Status = $"Signed in as {res.Data.DisplayName}";
            await RefreshAsync();
        }
        catch (HttpRequestException) { Status = "Server unreachable - working offline"; }
    }

    private async Task RefreshAsync()
    {
        Campaigns.Clear(); foreach (var c in await _api.GetCampaignsAsync()) Campaigns.Add(c);
        for (int page = 1; ; page++)
        {
            var p = await _api.GetAssetsAsync(page, 200);
            await _store.CacheAssetsAsync(p.Items);
            if (page * 200 >= p.TotalCount) break;
        }
        await LoadCacheAsync();
    }

    [RelayCommand]
    private void VerifyScan()
    {
        if (SelectedCampaign is null) { Status = "Select a campaign first"; return; }
        var tag = ScanInput.Trim(); if (tag.StartsWith("AMGH-ASSET:", StringComparison.OrdinalIgnoreCase)) tag = tag[11..];
        if (string.IsNullOrWhiteSpace(tag)) return;
        var known = Assets.Any(a => a.AssetTag.Equals(tag, StringComparison.OrdinalIgnoreCase));
        _store.Enqueue(new VerificationSubmission(Guid.NewGuid(), SelectedCampaign.Id, tag.ToUpperInvariant(),
            known ? VerificationResult.Verified : VerificationResult.Pending, VerificationMethod.Barcode, DateTime.UtcNow, null, null, known ? null : "Not in local cache", null));
        PendingCount = _store.PendingCount(); Status = known ? $"{tag} verified" : $"{tag} queued (unknown locally)"; ScanInput = "";
    }

    [RelayCommand]
    private async Task SyncAsync()
    {
        var o = await _sync.SyncAsync();
        PendingCount = _store.PendingCount();
        Status = o.Offline ? "Offline - will retry" : $"Synced {o.Accepted} accepted, {o.Rejected} rejected";
    }
}
