using System.Net.Http.Json;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;

namespace AMGH.ITInventory.Notifications;

public class GraphOptions
{
    public string TenantId { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string SenderMailbox { get; set; } = "";
    public string? TeamsWebhookUrl { get; set; }
}

public interface INotificationService
{
    Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
    Task SendTeamsAsync(string message, CancellationToken ct = default);
}

public class GraphNotificationService : INotificationService
{
    private readonly GraphOptions _o; private readonly ILogger<GraphNotificationService> _log; private readonly HttpClient _http;
    private GraphServiceClient? _graph;
    public GraphNotificationService(IOptions<GraphOptions> o, ILogger<GraphNotificationService> log, HttpClient http) { _o = o.Value; _log = log; _http = http; }

    private GraphServiceClient Graph => _graph ??= new GraphServiceClient(new ClientSecretCredential(_o.TenantId, _o.ClientId, _o.ClientSecret));

    public async Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_o.TenantId)) { _log.LogWarning("Graph not configured; email to {To} skipped.", to); return; }
        await Graph.Users[_o.SenderMailbox].SendMail.PostAsync(new SendMailPostRequestBody
        {
            Message = new Message
            {
                Subject = subject, Body = new ItemBody { ContentType = BodyType.Html, Content = htmlBody },
                ToRecipients = new List<Recipient> { new() { EmailAddress = new EmailAddress { Address = to } } }
            },
            SaveToSentItems = false
        }, cancellationToken: ct);
    }

    public async Task SendTeamsAsync(string message, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_o.TeamsWebhookUrl)) { _log.LogWarning("Teams webhook not configured."); return; }
        using var resp = await _http.PostAsJsonAsync(_o.TeamsWebhookUrl, new { text = message }, ct);
        resp.EnsureSuccessStatusCode();
    }
}

/// <summary>Entra ID user + Intune managed-device import via Microsoft Graph.</summary>
public interface IDirectorySyncService
{
    Task<IReadOnlyList<DirectoryUser>> GetUsersAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ManagedDeviceInfo>> GetIntuneDevicesAsync(CancellationToken ct = default);
}
public record DirectoryUser(string ObjectId, string DisplayName, string? Email, string? Department);
public record ManagedDeviceInfo(string DeviceId, string DeviceName, string? SerialNumber, string? Model, string? UserPrincipalName);

public class GraphDirectorySyncService : IDirectorySyncService
{
    private readonly GraphServiceClient _graph;
    public GraphDirectorySyncService(IOptions<GraphOptions> o)
        => _graph = new GraphServiceClient(new ClientSecretCredential(o.Value.TenantId, o.Value.ClientId, o.Value.ClientSecret));

    public async Task<IReadOnlyList<DirectoryUser>> GetUsersAsync(CancellationToken ct = default)
    {
        var result = new List<DirectoryUser>();
        var page = await _graph.Users.GetAsync(r => r.QueryParameters.Select = new[] { "id", "displayName", "mail", "department" }, ct);
        while (page is not null)
        {
            result.AddRange(page.Value!.Select(u => new DirectoryUser(u.Id!, u.DisplayName ?? "", u.Mail, u.Department)));
            if (page.OdataNextLink is null) break;
            page = await _graph.Users.WithUrl(page.OdataNextLink).GetAsync(cancellationToken: ct);
        }
        return result;
    }

    public async Task<IReadOnlyList<ManagedDeviceInfo>> GetIntuneDevicesAsync(CancellationToken ct = default)
    {
        var result = new List<ManagedDeviceInfo>();
        var page = await _graph.DeviceManagement.ManagedDevices.GetAsync(cancellationToken: ct);
        while (page is not null)
        {
            result.AddRange(page.Value!.Select(d => new ManagedDeviceInfo(d.Id!, d.DeviceName ?? "", d.SerialNumber, d.Model, d.UserPrincipalName)));
            if (page.OdataNextLink is null) break;
            page = await _graph.DeviceManagement.ManagedDevices.WithUrl(page.OdataNextLink).GetAsync(cancellationToken: ct);
        }
        return result;
    }
}
