using System.IO;
using System.Windows;
using AMGH.ITInventory.Desktop.Services;
using AMGH.ITInventory.Desktop.ViewModels;
using AMGH.ITInventory.Shared.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AMGH.ITInventory.Desktop;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var cfg = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("appsettings.json", optional: true).Build();
        var baseUrl = cfg["Api:BaseUrl"] ?? "https://localhost:7001/";
        var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AMGH.ITInventory", "offline.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        var sc = new ServiceCollection();
        sc.AddHttpClient<IAmghApiClient, AmghApiClient>(c => { c.BaseAddress = new Uri(baseUrl); c.Timeout = TimeSpan.FromSeconds(30); });
        sc.AddSingleton(new LocalStore(dbPath));
        sc.AddSingleton<IPendingVerificationStore>(sp => sp.GetRequiredService<LocalStore>());
        sc.AddSingleton(sp => new SyncEngine(sp.GetRequiredService<IAmghApiClient>(), sp.GetRequiredService<IPendingVerificationStore>(), Environment.MachineName));
        sc.AddSingleton<MainViewModel>();
        Services = sc.BuildServiceProvider();

        new MainWindow { DataContext = Services.GetRequiredService<MainViewModel>() }.Show();
    }
}
