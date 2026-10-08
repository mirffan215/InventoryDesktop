using AMGH.ITInventory.Application.Abstractions;
using AMGH.ITInventory.Application.Services;
using AMGH.ITInventory.Infrastructure.Discovery;
using AMGH.ITInventory.Notifications;
using AMGH.ITInventory.QRCode;
using AMGH.ITInventory.Reporting.Reports;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AMGH.ITInventory.Infrastructure.Services;

public class SystemClock : IClock { public DateTime UtcNow => DateTime.UtcNow; }

public class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _http;
    public HttpCurrentUser(IHttpContextAccessor http) => _http = http;
    public string? UserName => _http.HttpContext?.User.Identity?.Name;
    public string? IpAddress => _http.HttpContext?.Connection.RemoteIpAddress?.ToString();
}

public static class InfrastructureRegistration
{
    public static IServiceCollection AddAmghInfrastructure(this IServiceCollection s, IConfiguration config)
    {
        s.AddHttpContextAccessor();
        s.AddSingleton<IClock, SystemClock>();
        s.AddScoped<ICurrentUser, HttpCurrentUser>();
        s.AddScoped<IVerificationService, VerificationService>();
        s.AddSingleton<IQrService, QrService>();
        s.AddSingleton<IReportGenerator, ReportGenerator>();
        s.AddSingleton<INetworkDiscoveryService, NetworkDiscoveryService>();
        s.Configure<GraphOptions>(config.GetSection("Graph"));
        s.AddHttpClient<INotificationService, GraphNotificationService>();
        s.AddSingleton<IDirectorySyncService, GraphDirectorySyncService>();
        return s;
    }
}
