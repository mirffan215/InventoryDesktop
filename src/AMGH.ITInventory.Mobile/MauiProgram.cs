using AMGH.ITInventory.Mobile.Pages;
using AMGH.ITInventory.Mobile.Services;
using AMGH.ITInventory.Mobile.ViewModels;
using AMGH.ITInventory.Shared.Client;
using ZXing.Net.Maui.Controls;

namespace AMGH.ITInventory.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var b = MauiApp.CreateBuilder();
        b.UseMauiApp<App>().UseBarcodeReader();
        // Production base URL is set at build time; HTTPS only (cleartext is disabled in the Android manifest).
        b.Services.AddHttpClient<IAmghApiClient, AmghApiClient>(c => { c.BaseAddress = new Uri(AppConfig.ApiBaseUrl); c.Timeout = TimeSpan.FromSeconds(30); });
        b.Services.AddSingleton(new LocalDatabase(Path.Combine(FileSystem.AppDataDirectory, "amgh.db3")));
        b.Services.AddSingleton<IPendingVerificationStore>(sp => sp.GetRequiredService<LocalDatabase>());
        b.Services.AddSingleton(sp => new SyncEngine(sp.GetRequiredService<IAmghApiClient>(), sp.GetRequiredService<IPendingVerificationStore>(), DeviceInfo.Name + "-" + DeviceInfo.Model));
        b.Services.AddTransient<LoginViewModel>().AddTransient<LoginPage>();
        b.Services.AddTransient<ScanViewModel>().AddTransient<ScanPage>();
        return b.Build();
    }
}

public static class AppConfig { public const string ApiBaseUrl = "https://itinventory.example.local/"; }
