using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace SOPHOS_Host___Addresses_Manager;

internal static class AddressFeeds
{
    private const string DShield = "https://feeds.dshield.org/block.txt";
    private const string Drop = "https://www.spamhaus.org/drop/drop_v4.json";
    private const string Firehol = "https://raw.githubusercontent.com/firehol/blocklist-ipsets/master/firehol_level2.netset";
    private const string Oisd = "https://big.oisd.nl/";
    private const string StevenBlack = "https://raw.githubusercontent.com/StevenBlack/hosts/master/hosts";

    public static readonly (string Title, string Prefix, string Url)[] Templates =
    [
        ("DShield networks", "dshield", DShield),
        ("Spamhaus DROP (includes EDROP)", "spamhaus", Drop),
        ("FireHOL Level 2", "firehol-level2", Firehol),
        ("OISD Big domains", "oisd-big", Oisd),
        ("StevenBlack hosts", "stevenblack", StevenBlack)
    ];

    public static (string Url, string Format) Resolve(string value, string format)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != "https" || !uri.IsDefaultPort ||
            uri.UserInfo.Length != 0 || uri.Query.Length != 0 || uri.Fragment.Length != 0)
            return (value, format);

        return (uri.IdnHost, uri.AbsolutePath) switch
        {
            ("www.dshield.org" or "dshield.org" or "feeds.dshield.org", "/block.txt") => (DShield, "DShield"),
            ("www.spamhaus.org" or "spamhaus.org", "/drop/drop.txt" or "/drop/drop_v4.json") => (Drop, "Spamhaus"),
            ("www.spamhaus.org" or "spamhaus.org", "/drop/edrop.txt") => throw new FormatException(
                "Spamhaus merged EDROP into DROP. Use one Spamhaus job with " + Drop + "."),
            ("sslbl.abuse.ch", "/blacklist/sslipblacklist.txt") => throw new FormatException(
                "abuse.ch deprecated this SSLBL IP-only feed. Its URL now returns a notice instead of an address list."),
            ("raw.githubusercontent.com", "/ktsaou/blocklist-ipsets/master/firehol_level2.netset" or
                                          "/firehol/blocklist-ipsets/master/firehol_level2.netset") => (Firehol, "Feed TXT"),
            ("big.oisd.nl", "/") => (Oisd, "OISD"),
            ("raw.githubusercontent.com", "/StevenBlack/hosts/master/hosts") => (StevenBlack, "Hosts file"),
            _ => (value, format)
        };
    }

    public static IEnumerable<(string? Name, string Address)> Read(string input, string format)
    {
        using var reader = new StringReader(input);
        var lineNumber = 0;
        while (reader.ReadLine() is { } raw)
        {
            lineNumber++;
            var line = raw.Trim();
            if (line.Length == 0 || line[0] is '#' or ';') continue;
            if (format == "Hosts file")
            {
                foreach (var host in ReadHosts(line, lineNumber)) yield return (null, host);
                continue;
            }
            string? address = format switch
            {
                "DShield" => ReadDShield(line, lineNumber),
                "Spamhaus" => ReadSpamhaus(line, lineNumber),
                "OISD" => ReadOisd(line, lineNumber),
                "TXT" => System.Text.RegularExpressions.Regex.Replace(line, @"\s+@[A-Za-z0-9_-]+(?:\s+@[A-Za-z0-9_-]+)*$", ""),
                _ => line
            };
            if (address != null) yield return (null, address);
        }
    }

    private static string? ReadOisd(string line, int lineNumber)
    {
        if (line.StartsWith('!') || line == "[Adblock Plus]") return null;
        if (line.StartsWith("||", StringComparison.Ordinal) && line.EndsWith('^'))
        {
            var domain = line[2..^1];
            if (domain.Contains('.') && domain.All(c => char.IsLetterOrDigit(c) || c is '.' or '-'))
                return domain;
        }
        throw new FormatException($"Unsupported OISD domain rule on line {lineNumber}.");
    }

    private static IEnumerable<string> ReadHosts(string line, int lineNumber)
    {
        var columns = line.Split('#', 2)[0].Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (columns.Length < 2 || !IPAddress.TryParse(columns[0], out var address))
            throw new FormatException($"Invalid hosts-file record on line {lineNumber}.");
        if (!address.Equals(IPAddress.Any) && !address.Equals(IPAddress.IPv6Any) && !IPAddress.IsLoopback(address)) yield break;
        foreach (var host in columns.Skip(1).Where(host => host.Contains('.') && !IPAddress.TryParse(host, out _) &&
                     !host.TrimEnd('.').EndsWith(".localhost", StringComparison.OrdinalIgnoreCase)))
            yield return host;
    }

    private static string ReadDShield(string line, int lineNumber)
    {
        var columns = line.Split('\t');
        if (columns.Length < 3 || !IPAddress.TryParse(columns[0], out var start) ||
            !IPAddress.TryParse(columns[1], out var end) || start.AddressFamily != AddressFamily.InterNetwork ||
            end.AddressFamily != AddressFamily.InterNetwork ||
            !int.TryParse(columns[2], NumberStyles.None, CultureInfo.InvariantCulture, out var prefix) ||
            prefix is < 1 or > 32)
            throw new FormatException($"Invalid DShield network on line {lineNumber}.");
        var first = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(start.GetAddressBytes());
        var last = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(end.GetAddressBytes());
        var mask = uint.MaxValue << (32 - prefix);
        if ((first & mask) != first || (first | ~mask) != last)
            throw new FormatException($"DShield range and prefix disagree on line {lineNumber}.");
        return $"{start}/{prefix}";
    }

    private static string? ReadSpamhaus(string line, int lineNumber)
    {
        using var document = JsonDocument.Parse(line);
        var row = document.RootElement;
        if (row.TryGetProperty("cidr", out var cidr) && cidr.ValueKind == JsonValueKind.String &&
            cidr.GetString() is { } network && network.Contains('/'))
            return network;
        if (row.TryGetProperty("timestamp", out _) && row.TryGetProperty("copyright", out _))
            return null;
        throw new FormatException($"Invalid Spamhaus DROP record on line {lineNumber}.");
    }
}
