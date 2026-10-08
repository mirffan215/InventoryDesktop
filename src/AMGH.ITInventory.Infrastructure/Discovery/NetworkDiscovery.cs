using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;

namespace AMGH.ITInventory.Infrastructure.Discovery;

public record DiscoveredDevice(string IpAddress, string? HostName, bool IsAlive, long RoundTripMs, int[] OpenPorts, string? SnmpDescription, string DeviceType);

public interface INetworkDiscoveryService
{
    Task<IReadOnlyList<DiscoveredDevice>> ScanAsync(string cidr, string snmpCommunity = "public", int maxConcurrency = 64, CancellationToken ct = default);
}

public class NetworkDiscoveryService : INetworkDiscoveryService
{
    private static readonly int[] ProbePorts = { 22, 80, 135, 443, 445, 515, 631, 3389, 9100 };

    public static IEnumerable<IPAddress> ExpandCidr(string cidr)
    {
        var parts = cidr.Split('/');
        if (parts.Length != 2 || !IPAddress.TryParse(parts[0], out var ip) || ip.AddressFamily != AddressFamily.InterNetwork
            || !int.TryParse(parts[1], out var prefix) || prefix is < 16 or > 32)
            throw new ArgumentException("CIDR must be IPv4 with a prefix between /16 and /32.", nameof(cidr));
        uint addr = BitConverter.ToUInt32(ip.GetAddressBytes().Reverse().ToArray());
        uint mask = prefix == 0 ? 0 : uint.MaxValue << (32 - prefix);
        uint network = addr & mask, broadcast = network | ~mask;
        uint first = prefix >= 31 ? network : network + 1, last = prefix >= 31 ? broadcast : broadcast - 1;
        for (uint a = first; a <= last; a++) yield return new IPAddress(BitConverter.GetBytes(a).Reverse().ToArray());
    }

    public async Task<IReadOnlyList<DiscoveredDevice>> ScanAsync(string cidr, string snmpCommunity = "public", int maxConcurrency = 64, CancellationToken ct = default)
    {
        var results = new List<DiscoveredDevice>();
        using var gate = new SemaphoreSlim(maxConcurrency);
        var tasks = ExpandCidr(cidr).Select(async ip =>
        {
            await gate.WaitAsync(ct);
            try
            {
                var d = await ProbeAsync(ip, snmpCommunity, ct);
                if (d.IsAlive) lock (results) results.Add(d);
            }
            finally { gate.Release(); }
        }).ToList();
        await Task.WhenAll(tasks);
        return results.OrderBy(r => BitConverter.ToUInt32(IPAddress.Parse(r.IpAddress).GetAddressBytes().Reverse().ToArray())).ToList();
    }

    private static async Task<DiscoveredDevice> ProbeAsync(IPAddress ip, string community, CancellationToken ct)
    {
        using var ping = new Ping();
        PingReply reply;
        try { reply = await ping.SendPingAsync(ip, 800); } catch { return new(ip.ToString(), null, false, 0, Array.Empty<int>(), null, "Unknown"); }
        if (reply.Status != IPStatus.Success) return new(ip.ToString(), null, false, 0, Array.Empty<int>(), null, "Unknown");

        string? host = null;
        try { host = (await Dns.GetHostEntryAsync(ip)).HostName; } catch { }
        var ports = (await Task.WhenAll(ProbePorts.Select(p => IsPortOpenAsync(ip, p, ct)))).Where(x => x > 0).ToArray();
        var snmp = await GetSnmpDescriptionAsync(ip, community);
        return new(ip.ToString(), host, true, reply.RoundtripTime, ports, snmp, Fingerprint(ports, snmp));
    }

    private static async Task<int> IsPortOpenAsync(IPAddress ip, int port, CancellationToken ct)
    {
        try
        {
            using var c = new TcpClient();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct); cts.CancelAfter(500);
            await c.ConnectAsync(ip, port, cts.Token);
            return port;
        }
        catch { return 0; }
    }

    private static async Task<string?> GetSnmpDescriptionAsync(IPAddress ip, string community)
    {
        try
        {
            var r = await Messenger.GetAsync(VersionCode.V2, new IPEndPoint(ip, 161), new OctetString(community),
                new List<Variable> { new(new ObjectIdentifier("1.3.6.1.2.1.1.1.0")) }, CancellationToken.None);
            return r.FirstOrDefault()?.Data.ToString();
        }
        catch { return null; }
    }

    public static string Fingerprint(int[] ports, string? snmp)
    {
        var s = snmp?.ToLowerInvariant() ?? "";
        if (ports.Contains(9100) || ports.Contains(515) || ports.Contains(631) || s.Contains("printer")) return "Printer";
        if (s.Contains("cisco") || s.Contains("switch") || s.Contains("router") || s.Contains("fortigate")) return "Network Device";
        if (ports.Contains(3389) || ports.Contains(445) || ports.Contains(135)) return "Windows Host";
        if (ports.Contains(22)) return "Linux/Unix Host";
        return ports.Length > 0 ? "Network Host" : "Unknown";
    }
}
