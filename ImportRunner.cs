using System.Xml.Linq;
using System.Text.Json.Serialization;

namespace SOPHOS_Host___Addresses_Manager;

internal sealed record ImportSource(string Source, string Format, string Value, string? Prefix = null, int? MaxEntries = null)
{
    public override string ToString()
    {
        var name = Prefix ?? "Default name";
        var limit = MaxEntries is { } count ? $" (first {count:N0} entries)" : "";
        var input = Source == "Paste" ? $"Pasted {Format} ({Value.Length:N0} characters)" : $"{Format} · {Value}";
        return $"{name}{limit} · {input}";
    }
}

internal sealed record ImportSettings(string Origin, string Username, string Password, string ApiOverride,
    string Family, string Format, string Source, string Path, string PastedInput, string Prefix, string Operation,
    bool IpList, string RuleAction, string SourceZone, string DestinationZone, bool MatchSource, bool RuleEnabled,
    string? CertificateFingerprint = null, bool GroupIps = false, bool GroupFqdns = false,
    bool Wildcards = false, bool ReverseRule = false, ImportSource[]? Sources = null)
{
    [JsonIgnore]
    public Uri Endpoint => SophosApi.Endpoint(Origin);

    public SophosApi Api(bool nativeVersion = false) =>
        new(Endpoint, Username, Password, nativeVersion ? "" : ApiOverride,
            certificateFingerprint: CertificateFingerprint);

    [JsonIgnore]
    public ImportSource[] EffectiveSources => Sources is { Length: > 0 } ? Sources :
        [new ImportSource(Source, Format, Source == "Paste" ? PastedInput : Path)];

    public IEnumerable<string> AddressPrefixes() => EffectiveSources
        .Where(source => source.Format != "Entity XML / JSON").Select(source => source.Prefix ?? Prefix);

    public async Task<XElement[]> PlanAsync(string? apiVersion, CancellationToken cancellation)
    {
        var hosts = new Dictionary<string, List<XElement>>(StringComparer.OrdinalIgnoreCase);
        var imported = new List<XElement>();
        foreach (var source in EffectiveSources)
        {
            try
            {
                var (path, format) = source.Source == "HTTPS URL" && source.Format != "Entity XML / JSON"
                    ? AddressFeeds.Resolve(source.Value, source.Format) : (source.Value, source.Format);
                var input = source.Source == "Paste" ? source.Value :
                    await SophosApi.LoadAsync(path, source.Source == "HTTPS URL", cancellation);
                var entityFormat = format == "Entity XML / JSON";
                var prefix = source.Prefix ?? Prefix;
                var entries = await Task.Run(() => entityFormat ? TransferPlan.Entities(input) :
                    TransferPlan.Addresses(input, format, prefix, false, apiVersion, cancellation, Wildcards && !IpList,
                        source.MaxEntries), cancellation);
                if (entries.Length == 0) throw new FormatException("The source has no entities.");
                if (entityFormat) imported.AddRange(entries);
                else
                {
                    if (!hosts.TryGetValue(prefix, out var list)) hosts.Add(prefix, list = []);
                    list.AddRange(entries);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new InvalidOperationException($"Source {source}: {ex.Message}", ex);
            }
        }

        var grouped = new List<XElement>();
        var rules = new List<XElement>();
        if (hosts.Count == 0 && RuleAction != "None")
            throw new FormatException("Generated rules require at least one address source.");
        foreach (var (prefix, items) in hosts)
        {
            cancellation.ThrowIfCancellationRequested();
            var addresses = TransferPlan.MergeNamed(items);
            if (IpList) addresses = TransferPlan.IpList(addresses, prefix, apiVersion);
            var groups = IpList ? [] : TransferPlan.HostGroups(addresses, prefix, GroupIps, GroupFqdns);
            grouped.AddRange(addresses);
            grouped.AddRange(groups);
            if (RuleAction == "None") continue;
            var action = RuleAction == "Allow" ? "Accept" : "Drop";
            rules.AddRange(TransferPlan.LoggedRules(addresses, prefix, SourceZone, DestinationZone,
                MatchSource, RuleEnabled, action, groups: groups));
            if (ReverseRule)
                rules.AddRange(TransferPlan.LoggedRules(addresses, prefix, DestinationZone, SourceZone,
                    !MatchSource, RuleEnabled, action, reverse: true, groups: groups));
        }
        var entities = TransferPlan.MergeNamed(grouped.Concat(imported).Concat(rules));
        TransferPlan.Validate(entities, Operation, apiVersion);
        return entities;
    }
}

internal static class ImportRunner
{
    public static async Task<int> WriteAsync(SophosApi api, XElement[] entities, string mode,
        Action<int, string> progress, CancellationToken cancellation, IReadOnlyList<XElement>? retiredRules = null)
    {
        var completed = 0;
        foreach (var (entity, requested) in entities.Select(item => (item, mode))
            .Concat((retiredRules ?? []).Select(item => (item, "retire"))))
        {
            cancellation.ThrowIfCancellationRequested();
            var name = (string?)entity.Element("Name");
            var operation = requested;
            try
            {
                if (operation is "upsert" or "retire" || operation == "update" && IsManagedRule(entity))
                {
                    var query = new XElement(entity.Name, new XElement("Filter", new XElement("key",
                        new XAttribute("name", "Name"), new XAttribute("criteria", "="), name)));
                    var response = await api.SendAsync(new XElement("Get", query), cancellation);
                    var existing = response.Elements(entity.Name).FirstOrDefault(item =>
                        string.Equals((string?)item.Element("Name"), name, StringComparison.OrdinalIgnoreCase));
                    if (operation == "retire" && existing == null)
                    {
                        progress(++completed, $"Already absent: {entity.Name} {name}");
                        continue;
                    }
                    if (existing != null && IsManagedRule(entity) && !IsManagedRule(existing))
                        throw new InvalidOperationException("A rule with this name exists outside this app's management.");
                    if (existing != null && (IsCurrentIpList(entity, existing) || IsCurrentHostOrGroup(entity, existing)))
                    {
                        progress(++completed, $"Unchanged: {entity.Name} {name}");
                        continue;
                    }
                    if (operation == "retire" && (string?)existing?.Element("Status") == "Disable")
                    {
                        progress(++completed, $"Already disabled: {entity.Name} {name}");
                        continue;
                    }
                    operation = existing != null || requested == "update" ? "update" : "add";
                }
                await api.SetAsync(operation, entity, cancellation);
                progress(++completed, $"{operation} {entity.Name} {name}");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new InvalidOperationException(
                    $"Stopped after {completed} completed item(s) at {entity.Name} {name}. {ex.Message}", ex);
            }
        }
        return completed;
    }

    private static bool IsManagedRule(XElement entity) => entity.Name == "FirewallRule" &&
        (string?)entity.Element("Description") == TransferPlan.ManagedRuleDescription;

    private static bool IsCurrentIpList(XElement desired, XElement existing)
    {
        if (desired.Name != "IPHost" || (string?)desired.Element("HostType") != "IPList" ||
            (string?)existing.Element("HostType") != "IPList" || desired.Elements().Any(e =>
                e.Name.LocalName is not ("Name" or "HostType" or "IPFamily" or "ListOfIPAddresses")) ||
            desired.HasAttributes || desired.Elements().Any(e => e.HasAttributes) ||
            (string?)desired.Element("IPFamily") != (string?)existing.Element("IPFamily")) return false;
        var requested = ((string?)desired.Element("ListOfIPAddresses") ?? "").Split(',', StringSplitOptions.TrimEntries);
        var current = ((string?)existing.Element("ListOfIPAddresses") ?? "").Split(',', StringSplitOptions.TrimEntries);
        return requested.ToHashSet(StringComparer.Ordinal).SetEquals(current);
    }

    private static bool IsCurrentHostOrGroup(XElement desired, XElement existing)
    {
        if (desired.HasAttributes || desired.Elements().Any(child => child.HasAttributes)) return false;
        if (desired.Name == "IPHost" && desired.Elements().All(child => !child.HasElements &&
            child.Name.LocalName is "Name" or "IPFamily" or "HostType" or "IPAddress" or "Subnet" or "StartIPAddress" or "EndIPAddress") &&
            (string?)desired.Element("HostType") is "IP" or "Network" or "IPRange")
            return desired.Elements().Where(child => child.Name != "Name").All(child =>
                string.Equals(child.Value, (string?)existing.Element(child.Name), StringComparison.OrdinalIgnoreCase));
        if (desired.Name == "FQDNHost" && desired.Elements().Count() == 2 && desired.Element("FQDN") is { } domain)
            return string.Equals(domain.Value, (string?)existing.Element("FQDN"), StringComparison.OrdinalIgnoreCase);
        var listName = desired.Name.LocalName switch
        {
            "IPHostGroup" => "HostList",
            "FQDNHostGroup" => "FQDNHostList",
            _ => null
        };
        if (listName == null || desired.Elements().Any(child => child.Name != "Name" &&
            child.Name != "IPFamily" && child.Name != listName) ||
            (string?)desired.Element("IPFamily") != (string?)existing.Element("IPFamily") ||
            desired.Element(listName) is not { } requested || existing.Element(listName) is not { } current ||
            requested.Elements().Any(member => member.HasAttributes || member.HasElements)) return false;
        return requested.Elements().Select(member => (member.Name, member.Value.ToUpperInvariant())).ToHashSet()
            .SetEquals(current.Elements().Select(member => (member.Name, member.Value.ToUpperInvariant())));
    }

    public static IDisposable LockEndpoint(Uri endpoint)
    {
        var key = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(endpoint.AbsoluteUri.ToUpperInvariant())));
        return ScheduleStore.LockFile("endpoint-" + key);
    }
}

