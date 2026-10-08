using System.Collections.ObjectModel;
using AMGH.ITInventory.Mobile.Services;
using AMGH.ITInventory.Shared.Client;
using AMGH.ITInventory.Shared.Contracts;
using AMGH.ITInventory.Shared.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AMGH.ITInventory.Mobile.ViewModels;

public partial class ScanViewModel : ObservableObject
{
    private readonly IAmghApiClient _api; private readonly LocalDatabase _db; private readonly SyncEngine _sync;
    private string? _lastTag; private DateTime _lastScanUtc;

    [ObservableProperty] private CampaignDto? _selectedCampaign;
    [ObservableProperty] private string _message = "Select a campaign, then scan an asset QR/barcode";
    [ObservableProperty] private int _pending;
    [ObservableProperty] private string? _photoPath;
    [ObservableProperty] private string _comment = "";
    public ObservableCollection<CampaignDto> Campaigns { get; } = new();

    public ScanViewModel(IAmghApiClient api, LocalDatabase db, SyncEngine sync) { _api = api; _db = db; _sync = sync; _ = InitAsync(); }

    private async Task InitAsync()
    {
        try
        {
            foreach (var c in (await _api.GetCampaignsAsync()).Where(c => c.Status == CampaignStatus.Active)) Campaigns.Add(c);
            for (int page = 1; ; page++)
            {
                var p = await _api.GetAssetsAsync(page, 200); await _db.CacheAssetsAsync(p.Items);
                if (page * 200 >= p.TotalCount) break;
            }
        }
        catch (HttpRequestException) { Message = "Offline - using cached assets"; }
        Pending = await _db.PendingCountAsync();
    }

    /// <summary>Called by the camera view for each decoded code; debounced so one tag is not recorded repeatedly.</summary>
    public async Task OnCodeScannedAsync(string raw)
    {
        if (SelectedCampaign is null) { Message = "Select a campaign first"; return; }
        var tag = raw.Trim(); if (tag.StartsWith("AMGH-ASSET:", StringComparison.OrdinalIgnoreCase)) tag = tag[11..];
        tag = tag.ToUpperInvariant();
        if (tag == _lastTag && (DateTime.UtcNow - _lastScanUtc).TotalSeconds < 3) return;
        _lastTag = tag; _lastScanUtc = DateTime.UtcNow;

        double? lat = null, lon = null;
        try { var loc = await Geolocation.Default.GetLastKnownLocationAsync() ?? await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(5))); lat = loc?.Latitude; lon = loc?.Longitude; }
        catch (Exception) { /* GPS is best-effort */ }

        var asset = await _db.FindAssetAsync(tag);
        await _db.EnqueueAsync(new PendingRow
        {
            ClientId = Guid.NewGuid().ToString(), CampaignId = SelectedCampaign.Id, AssetTag = tag,
            Result = (int)(asset is null ? VerificationResult.Missing : VerificationResult.Verified), Method = (int)VerificationMethod.Qr,
            VerifiedAtUtc = DateTime.UtcNow, Latitude = lat, Longitude = lon, Comment = asset is null ? "Unknown tag scanned" : (string.IsNullOrWhiteSpace(Comment) ? null : Comment), PhotoPath = PhotoPath
        });
        Message = asset is null ? $"{tag}: not found in cache (recorded as exception)" : $"{tag}: {asset.Name} - verified";
        PhotoPath = null; Comment = ""; Pending = await _db.PendingCountAsync();
    }

    [RelayCommand]
    private async Task CapturePhotoAsync()
    {
        if (!MediaPicker.Default.IsCaptureSupported) return;
        var photo = await MediaPicker.Default.CapturePhotoAsync();
        if (photo is null) return;
        var dest = Path.Combine(FileSystem.CacheDirectory, $"{Guid.NewGuid():N}.jpg");
        await using (var src = await photo.OpenReadAsync()) await using (var dst = File.Create(dest)) await src.CopyToAsync(dst);
        PhotoPath = dest; Message = "Photo attached to next scan";
    }

    [RelayCommand]
    private async Task SyncAsync()
    {
        var o = await _sync.SyncAsync();
        Pending = await _db.PendingCountAsync();
        Message = o.Offline ? "Offline - data kept on device" : $"Synced: {o.Accepted} accepted, {o.Rejected} rejected";
    }
}
