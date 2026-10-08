using System.Net.Http.Headers;
using System.Net.Http.Json;
using AMGH.ITInventory.Shared.Contracts;

namespace AMGH.ITInventory.Shared.Client;

public interface IAmghApiClient
{
    string? AccessToken { get; set; }
    Task<ApiResponse<LoginResponse>?> LoginAsync(string user, string password, CancellationToken ct = default);
    Task<IReadOnlyList<CampaignDto>> GetCampaignsAsync(CancellationToken ct = default);
    Task<AssetDto?> GetAssetByTagAsync(string tag, CancellationToken ct = default);
    Task<PagedResult<AssetDto>> GetAssetsAsync(int page, int pageSize, CancellationToken ct = default);
    Task<SyncPushResult> PushVerificationsAsync(SyncPushRequest request, CancellationToken ct = default);
}

public class AmghApiClient : IAmghApiClient
{
    private const string V = "api/v1/";
    private readonly HttpClient _http;
    public AmghApiClient(HttpClient http) => _http = http;

    public string? AccessToken
    {
        get => _http.DefaultRequestHeaders.Authorization?.Parameter;
        set => _http.DefaultRequestHeaders.Authorization = value is null ? null : new AuthenticationHeaderValue("Bearer", value);
    }

    public async Task<ApiResponse<LoginResponse>?> LoginAsync(string user, string password, CancellationToken ct = default)
    {
        using var r = await _http.PostAsJsonAsync(V + "auth/token", new LoginRequest(user, password), ct);
        if (!r.IsSuccessStatusCode) return null;
        var res = await r.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(cancellationToken: ct);
        if (res?.Data is not null) AccessToken = res.Data.AccessToken;
        return res;
    }

    public async Task<IReadOnlyList<CampaignDto>> GetCampaignsAsync(CancellationToken ct = default) =>
        (await _http.GetFromJsonAsync<ApiResponse<List<CampaignDto>>>(V + "verification/campaigns", ct))?.Data ?? new();

    public async Task<AssetDto?> GetAssetByTagAsync(string tag, CancellationToken ct = default)
    {
        using var r = await _http.GetAsync(V + "assets/by-tag/" + Uri.EscapeDataString(tag), ct);
        return r.IsSuccessStatusCode ? (await r.Content.ReadFromJsonAsync<ApiResponse<AssetDto>>(cancellationToken: ct))?.Data : null;
    }

    public async Task<PagedResult<AssetDto>> GetAssetsAsync(int page, int pageSize, CancellationToken ct = default) =>
        (await _http.GetFromJsonAsync<ApiResponse<PagedResult<AssetDto>>>($"{V}assets?page={page}&pageSize={pageSize}", ct))?.Data ?? new PagedResult<AssetDto>();

    public async Task<SyncPushResult> PushVerificationsAsync(SyncPushRequest request, CancellationToken ct = default)
    {
        using var r = await _http.PostAsJsonAsync(V + "verification/sync", request, ct);
        r.EnsureSuccessStatusCode();
        return (await r.Content.ReadFromJsonAsync<ApiResponse<SyncPushResult>>(cancellationToken: ct))!.Data!;
    }
}
