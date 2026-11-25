using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using Microsoft.VisualBasic.FileIO;

namespace SOPHOS_Host___Addresses_Manager;

internal static class TransferPlan
{
    public static XName EntityTag(string tag)
    {
        XmlConvert.VerifyNCName(tag);
        if (tag is "Request" or "Response" or "Entities" or "Login" or "Set" or "Get" or "Remove" or "Status")
            throw new FormatException("Enter a Sophos entity tag, such as IPHost.");
        return XName.Get(tag);
    }

    public const int RuleChunkSize = 50;
    // SFOS 22.0 GA had a policy-matching issue above 256 members (NC-171031).
    public const int GroupChunkSize = 256;
    public const string ManagedRuleDescription = "Managed by Sophos Firewall Object Manager";
    public const int MaxHostObjects = 16000;
    public static int IpListLimit(string? apiVersion) =>
        apiVersion?.StartsWith("1900.", StringComparison.Ordinal) == true ? 1000 : 800;

    public static void Validate(XElement[] entities, string operation, string? apiVersion)
    {
        if (entities.Length == 0) throw new FormatException("The import has no entities.");
        if (entities.Count(entity => entity.Name.LocalName is "IPHost" or "MACHost" or "FQDNHost" or "CountryGroup" or "CountryHostGroup") > MaxHostObjects)
            throw new FormatException($"The plan exceeds Sophos's {MaxHostObjects:N0}-host ceiling across host types.");
        var names = new HashSet<(XName, string)>();
        foreach (var entity in entities)
        {
            if (entity.Name.Namespace != XNamespace.None) throw new FormatException("Entity tags must have no namespace.");
            EntityTag(entity.Name.LocalName);
            if (entity.Name == "ThirdPartyFeed")
                throw new FormatException("Use a file or HTTPS URL to refresh address objects with this app.");
            var name = (string?)entity.Element("Name");
            if (operation == "upsert" && string.IsNullOrWhiteSpace(name))
                throw new FormatException("Upsert requires a Name on every entity. Use Add or Update for other schemas.");
            if (!string.IsNullOrWhiteSpace(name) && !names.Add((entity.Name, name.Trim().ToUpperInvariant())))
                throw new FormatException($"Duplicate entity {entity.Name} {name}.");
            if (entity.Name == "IPHost" && ((string?)entity.Element("ListOfIPAddresses"))?.Split(',').Length > IpListLimit(apiVersion))
                throw new FormatException($"An IP list on this firmware can contain up to {IpListLimit(apiVersion)} entries.");
            if (entity.Name == "MACHost" && entity.Element("MACList")?.Elements("MACAddress").Count() > 1000)
                throw new FormatException("A Sophos MAC list can contain up to 1,000 MAC addresses.");
        }
    }

    public static XElement[] Entities(string input)
    {
        if (input.TrimStart().StartsWith('{'))
        {
            using var json = JsonDocument.Parse(input);
            return json.RootElement.GetProperty("entities").EnumerateArray()
                .Select(item => SophosApi.Parse(item.GetString() ?? "")).ToArray();
        }

        var root = SophosApi.Parse(input);
        return root.Name == "Entities" ? root.Elements().ToArray() : [root];
    }

    public static XElement[] Addresses(string input, string format, string prefix, bool ipList, string? apiVersion,
        CancellationToken cancellation, bool wildcards = false, int? maxEntries = null)
    {
        if (prefix.Length is < 1 or > 30 || prefix.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '-' && c != '_'))
            throw new FormatException("Use a 1–30 character prefix containing letters, digits, hyphens or underscores.");
        IEnumerable<(string? Name, string Address)> rows = format switch
        {
            "JSON" => JsonRows(input),
            "CSV" => CsvRows(input),
            _ => AddressFeeds.Read(input, format)
        };
        if (maxEntries is { } limit)
            rows = rows.Distinct().Take(limit);
        var objects = new Dictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
        var rowNumber = 0;
        foreach (var (name, address) in rows)
        {
            cancellation.ThrowIfCancellationRequested();
            rowNumber++;
            try
            {
                var entity = Address(name, address, prefix, wildcards);
                Add(entity);
                if (wildcards && entity.Element("FQDN") is { } fqdn && !fqdn.Value.StartsWith("*."))
                    Add(Address(null, "*." + fqdn.Value, prefix, true));
                if (objects.Count > MaxHostObjects)
                    throw new FormatException($"The source exceeds Sophos's {MaxHostObjects:N0}-host ceiling. " +
                        "Use a smaller feed or explicitly set a source entry limit.");
            }
            catch (FormatException ex) { throw new FormatException($"Address entry {rowNumber}: {ex.Message}", ex); }
        }
        return ipList ? IpList(objects.Values, prefix, apiVersion) : objects.Values.ToArray();

        void Add(XElement entity)
        {
            var key = entity.Element("Name")!.Value;
            if (objects.TryGetValue(key, out var existing) && !XNode.DeepEquals(existing, entity))
                throw new FormatException($"The name '{key}' has conflicting addresses.");
            objects[key] = entity;
        }
    }

    public static XElement[] IpList(IEnumerable<XElement> hosts, string prefix, string? apiVersion)
    {
        var addresses = hosts.Select(item => (string?)item.Element("HostType") == "IP" &&
                (string?)item.Element("IPFamily") == "IPv4" && item.Element("IPAddress") is { } address
                    ? address.Value : throw new FormatException("An IPv4 address list requires individual IPv4 addresses."))
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        if (addresses.Length == 0) return [];
        if (addresses.Length > IpListLimit(apiVersion))
            throw new FormatException($"A single IPv4 address list supports {IpListLimit(apiVersion)} entries. Split the input into separate named lists.");
        return [new XElement("IPHost", new XElement("Name", prefix), new XElement("IPFamily", "IPv4"),
            new XElement("HostType", "IPList"), new XElement("ListOfIPAddresses", string.Join(",", addresses)))];
    }

    public static XElement[] MergeNamed(IEnumerable<XElement> entities)
    {
        var result = new List<XElement>();
        var names = new Dictionary<(XName, string), XElement>();
        foreach (var entity in entities)
        {
            var name = (string?)entity.Element("Name");
            if (!string.IsNullOrWhiteSpace(name))
            {
                var key = (entity.Name, name.Trim().ToUpperInvariant());
                if (names.TryGetValue(key, out var prior))
                {
                    if (!XNode.DeepEquals(prior, entity))
                        throw new FormatException($"Sources contain conflicting definitions for {entity.Name} {name}.");
                    continue;
                }
                names.Add(key, entity);
            }
            result.Add(entity);
        }
        return result.ToArray();
    }

    private static IEnumerable<(string?, string)> JsonRows(string input)
    {
        using var json = JsonDocument.Parse(input);
        foreach (var item in json.RootElement.EnumerateArray())
            yield return item.ValueKind == JsonValueKind.String ? (null, item.GetString()!) :
                (item.TryGetProperty("name", out var name) ? name.GetString() : null,
                item.GetProperty("address").GetString() ?? "");
    }

    private static IEnumerable<(string?, string)> CsvRows(string input)
    {
        using var parser = new TextFieldParser(new StringReader(input));
        parser.SetDelimiters(",");
        parser.HasFieldsEnclosedInQuotes = true;
        var header = parser.ReadFields() ?? [];
        var address = Array.FindIndex(header, column => column.Equals("address", StringComparison.OrdinalIgnoreCase));
        var name = Array.FindIndex(header, column => column.Equals("name", StringComparison.OrdinalIgnoreCase));
        if (address < 0) throw new FormatException("CSV requires an address column. The name column is optional.");
        while (!parser.EndOfData)
        {
            var fields = parser.ReadFields() ?? [];
            if (fields.Length != header.Length) throw new FormatException("A CSV row has the wrong number of columns.");
            yield return (name < 0 ? null : fields[name], fields[address]);
        }
    }

    private static XElement Address(string? name, string raw, string prefix, bool wildcards)
    {
        var value = raw.Trim();
        var parts = value.Split('/');
        XElement entity;
        if (TryAddress(parts[0], out var start))
        {
            CheckAddress(start);
            if (parts.Length == 1)
                entity = Ip(start, "IP", new XElement("IPAddress", start));
            else
            {
                var bytes = start.GetAddressBytes();
                if (parts.Length != 2 || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var bits)
                    || bits < 0 || bits > bytes.Length * 8)
                    throw new FormatException("Invalid CIDR prefix.");
                var mask = new byte[bytes.Length];
                for (int i = 0; i < bits; i++) mask[i / 8] |= (byte)(128 >> (i % 8));
                for (int i = 0; i < bytes.Length; i++) bytes[i] &= mask[i];
                entity = bytes.Length == 4
                    ? Ip(start, "Network", new XElement("IPAddress", new IPAddress(bytes)),
                        new XElement("Subnet", new IPAddress(mask)))
                    : Ip(start, "IPRange", new XElement("StartIPAddress", new IPAddress(bytes)),
                        new XElement("EndIPAddress", new IPAddress(bytes.Select((b, i) => (byte)(b | ~mask[i])).ToArray())));
            }
        }
        else if (value.Split('-', 2) is [var first, var last] &&
                 TryAddress(first.Trim(), out start) && TryAddress(last.Trim(), out var end))
        {
            CheckAddress(start);
            CheckAddress(end);
            if (start.AddressFamily != end.AddressFamily ||
                start.GetAddressBytes().AsSpan().SequenceCompareTo(end.GetAddressBytes()) > 0)
                throw new FormatException("IP range endpoints must be ordered and use the same address family.");
            entity = Ip(start, "IPRange", new XElement("StartIPAddress", start), new XElement("EndIPAddress", end));
        }
        else
        {
            var wildcard = value.StartsWith("*.", StringComparison.Ordinal);
            if (wildcard && !wildcards)
                throw new FormatException("The input contains a wildcard. Select Add FQDN wildcards to approve wildcard import.");
            var domain = new IdnMapping().GetAscii((wildcard ? value[2..] : value).TrimEnd('.')).ToLowerInvariant();
            if (!domain.Contains('.') || Uri.CheckHostName(domain) != UriHostNameType.Dns ||
                domain.Split('.').Any(label => label.Length is < 1 or > 63 || label.StartsWith('-') ||
                    label.EndsWith('-') || label.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '-')) ||
                domain.Length > (wildcard ? 251 : 253) || domain.All(c => char.IsAsciiDigit(c) || c == '.'))
                throw new FormatException("Expected an IP, CIDR, IP range or fully qualified domain name.");
            entity = new XElement("FQDNHost", new XElement("FQDN", wildcard ? "*." + domain : domain));
        }
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(entity.ToString(SaveOptions.DisableFormatting))))[..16].ToLowerInvariant();
        var objectName = string.IsNullOrWhiteSpace(name) ? $"{prefix}-{hash}" : name.Trim();
        if (objectName.Length > 50 || objectName.Any(char.IsControl))
            throw new FormatException("Host names must be 50 characters or fewer with no control characters.");
        entity.AddFirst(new XElement("Name", objectName));
        return entity;
    }

    private static bool TryAddress(string value, out IPAddress address)
    {
        address = IPAddress.None;
        if (!IPAddress.TryParse(value, out var parsed)) return false;
        if (parsed.AddressFamily == AddressFamily.InterNetwork && value != parsed.ToString()) return false;
        address = parsed;
        return true;
    }

    private static void CheckAddress(IPAddress address)
    {
        if (address.AddressFamily == AddressFamily.InterNetworkV6 && address.ScopeId != 0)
            throw new FormatException("Scoped IPv6 addresses are unsupported.");
    }

    private static XElement Ip(IPAddress address, string type, params XElement[] fields) =>
        new("IPHost", new XElement("IPFamily", address.AddressFamily == AddressFamily.InterNetwork ? "IPv4" : "IPv6"),
            new XElement("HostType", type), fields);

    public static XElement[] HostGroups(XElement[] hosts, string prefix, bool groupIps, bool groupFqdns)
    {
        var groups = new List<XElement>();
        if (groupIps)
            foreach (var family in new[] { "IPv4", "IPv6" })
                Add(hosts.Where(host => host.Name == "IPHost" && (string?)host.Element("IPFamily") == family &&
                    (string?)host.Element("HostType") != "IPList"), "IPHostGroup", family, "HostList", "Host");
        if (groupFqdns) Add(hosts.Where(host => host.Name == "FQDNHost"), "FQDNHostGroup", null, "FQDNHostList", "FQDNHost");
        return groups.ToArray();

        void Add(IEnumerable<XElement> candidates, string type, string? family, string list, string member)
        {
            var names = candidates.Select(host => host.Element("Name")!.Value).Order(StringComparer.OrdinalIgnoreCase).ToArray();
            if (names.Length < 2) return;
            var number = 1;
            foreach (var chunk in names.Chunk(GroupChunkSize))
                groups.Add(new XElement(type, new XElement("Name", $"{prefix}-{family?.ToLowerInvariant() ?? "fqdn"}-{number++}"),
                    family == null ? null : new XElement("IPFamily", family),
                    new XElement(list, chunk.Select(name => new XElement(member, name)))));
        }
    }

    public static XElement[] LoggedRules(XElement[] hosts, string prefix, string source, string destination,
        bool matchSource, bool enabled, string action, bool reverse = false, XElement[]? groups = null)
    {
        if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(destination))
            throw new FormatException("Enter both source and destination zones for logged rules.");
        if (hosts.Any(host => string.Equals((string?)host.Element("Name"), "Any", StringComparison.OrdinalIgnoreCase)))
            throw new FormatException("An imported object named Any cannot be used in a generated rule.");
        if (hosts.Any(host => (string?)host.Element("Subnet") == "0.0.0.0" ||
            (string?)host.Element("Prefix") == "0" ||
            ((string?)host.Element("StartIPAddress"), (string?)host.Element("EndIPAddress")) is
                ("0.0.0.0", "255.255.255.255") or ("::", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")))
            throw new FormatException("A rule entry covers an entire address family.");
        var rules = new List<XElement>();
        groups ??= [];
        var covered = groups.SelectMany(group => group.Descendants("Host")
            .Select(member => (Type: "IPHost", Name: member.Value)).Concat(group.Descendants("FQDNHost")
            .Select(member => (Type: "FQDNHost", Name: member.Value)))).ToHashSet();
        var targets = hosts.Where(host => !covered.Contains((host.Name.LocalName, host.Element("Name")!.Value))).Concat(groups);
        foreach (var family in new[] { "IPv4", "IPv6" })
        {
            var matching = targets.Where(host => host.Name == "FQDNHost" || host.Name == "FQDNHostGroup" ||
                (string?)host.Element("IPFamily") == family);
            var part = 0;
            foreach (var chunk in matching.Chunk(RuleChunkSize))
            {
                var names = chunk.Select(host => new XElement("Network", host.Element("Name")!.Value));
                rules.Add(new XElement("FirewallRule", new XAttribute("transactionid", ""),
                    new XElement("Name", $"{prefix}-logged-{family}{(reverse ? "-reverse" : "")}-{++part:D3}"),
                    new XElement("Description", ManagedRuleDescription),
                    new XElement("IPFamily", family), new XElement("Status", enabled ? "Enable" : "Disable"),
                    new XElement("Position", action == "Accept" ? "Bottom" : "Top"), new XElement("PolicyType", "Network"),
                    new XElement("NetworkPolicy", new XElement("Action", action), new XElement("LogTraffic", "Enable"),
                        new XElement("SkipLocalDestined", "Disable"),
                        source.Equals("Any", StringComparison.OrdinalIgnoreCase) ? null : new XElement("SourceZones", new XElement("Zone", source.Trim())),
                        destination.Equals("Any", StringComparison.OrdinalIgnoreCase) ? null : new XElement("DestinationZones", new XElement("Zone", destination.Trim())),
                        new XElement("Schedule", "All The Time"),
                        new XElement(matchSource ? "SourceNetworks" : "DestinationNetworks", names))));
            }
        }
        return rules.ToArray();
    }

}

