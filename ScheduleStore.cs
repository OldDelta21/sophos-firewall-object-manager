using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text.Json;
using System.Xml.Linq;

namespace SOPHOS_Host___Addresses_Manager;

internal sealed record ScheduledImport(Guid Id, string Name, int Minutes, bool Enabled, ImportSettings Settings,
    DateTime? StartAt = null);
internal sealed record ImportResult(DateTimeOffset Started, DateTimeOffset Finished, string Message, IReadOnlyList<string> Activity);

internal static class ScheduleStore
{
    internal static readonly string DataDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SophosFirewallObjectManager");
    private static readonly string DirectoryPath = Path.Combine(DataDirectory, "Jobs");

    private static string Location(Guid id, string kind) => Path.Combine(DirectoryPath, $"{id:N}.{kind}");

    public static IDisposable LockFile(string name)
    {
        Directory.CreateDirectory(DirectoryPath);
        try { return File.Open(Path.Combine(DirectoryPath, name + ".lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
        catch (IOException ex) { throw new InvalidOperationException("Another import or task edit is already using this target.", ex); }
    }

    public static ScheduledImport[] List()
    {
        Directory.CreateDirectory(DirectoryPath);
        return Directory.EnumerateFiles(DirectoryPath, "*.job")
            .Select(file => Read<ScheduledImport>(file)).OrderBy(job => job.Name, StringComparer.CurrentCultureIgnoreCase).ToArray();
    }

    public static ImportResult[] History(Guid id)
    {
        var file = Location(id, "history");
        return File.Exists(file) ? Read<ImportResult[]>(file) : [];
    }

    public static async Task SaveAsync(ScheduledImport job)
    {
        using var editing = LockFile(job.Id.ToString("N"));
        using var catalog = LockFile("catalog");
        if (List().Any(other => other.Id != job.Id && other.Settings.Endpoint == job.Settings.Endpoint &&
            other.Settings.AddressPrefixes().Intersect(job.Settings.AddressPrefixes(), StringComparer.OrdinalIgnoreCase).Any()))
            throw new InvalidOperationException("A scheduled import already owns this address prefix on this firewall.");
        var file = Location(job.Id, "job");
        var previous = File.Exists(file) ? Read<ScheduledImport>(file) : null;
        if (previous != null && (previous.Settings.Endpoint != job.Settings.Endpoint ||
            !previous.Settings.Prefix.Equals(job.Settings.Prefix, StringComparison.OrdinalIgnoreCase) ||
            previous.Settings.IpList != job.Settings.IpList))
            throw new InvalidOperationException("Create a new task to change the target firewall, object prefix, or address representation.");
        Write(file, job);
        try { await RegisterAsync(job); }
        catch
        {
            if (previous == null) File.Delete(file);
            else Write(file, previous);
            throw;
        }
    }

    public static async Task DeleteAsync(Guid id)
    {
        using var editing = LockFile(id.ToString("N"));
        using var catalog = LockFile("catalog");
        await SchedulerAsync("/delete", "/tn", TaskName(id), "/f");
        File.Delete(Location(id, "job"));
        File.Delete(Location(id, "history"));
        File.Delete(Location(id, "rules"));
    }

    public static async Task SetEnabledAsync(Guid id, bool enabled)
    {
        using var editing = LockFile(id.ToString("N"));
        var file = Location(id, "job");
        var previous = Read<ScheduledImport>(file);
        previous = previous with { Settings = CurrentConnection(previous) };
        Write(file, previous with { Enabled = enabled });
        try { await SchedulerAsync("/change", "/tn", TaskName(id), enabled ? "/enable" : "/disable"); }
        catch
        {
            Write(file, previous);
            throw;
        }
    }

    public static async Task<bool> RunAsync(Guid id, bool manual, Action<int, int, string>? progress, CancellationToken cancellation)
    {
        using var running = LockFile(id.ToString("N"));
        var job = Read<ScheduledImport>(Location(id, "job"));
        if (!job.Enabled && !manual) return true;
        var started = DateTimeOffset.Now;
        var activity = new Queue<string>();
        string message;
        bool success;
        try
        {
            var settings = CurrentConnection(job);
            using var target = ImportRunner.LockEndpoint(settings.Endpoint);
            using var api = settings.Api();
            using var nativeApi = settings.Api(nativeVersion: true);
            var response = await nativeApi.SendAsync(null, cancellation);
            var apiVersion = (string?)response.Attribute("APIVersion");
            var plan = await job.Settings.PlanAsync(apiVersion, cancellation);
            var rulesFile = Location(id, "rules");
            var previous = File.Exists(rulesFile) ? Read<string[]>(rulesFile).Select(SophosApi.Parse).ToArray() : [];
            var current = job.Settings.RuleAction == "None" ? [] : plan.Where(e => e.Name == "FirewallRule" &&
                (string?)e.Element("Description") == TransferPlan.ManagedRuleDescription).ToArray();
            var known = previous.Concat(current).DistinctBy(e => ((string?)e.Element("Name"))?.ToUpperInvariant()).ToArray();
            var retired = previous.Where(old => !current.Any(rule => string.Equals(
                    (string?)old.Element("Name"), (string?)rule.Element("Name"), StringComparison.OrdinalIgnoreCase)))
                .Select(rule =>
                {
                    var disabled = new XElement(rule);
                    disabled.SetElementValue("Status", "Disable");
                    return disabled;
                }).ToArray();
            Write(rulesFile, known.Select(e => e.ToString(SaveOptions.DisableFormatting)).ToArray());
            var total = plan.Length + retired.Length;
            progress?.Invoke(0, total, "Importing refreshed configuration…");
            var confirmed = await ImportRunner.WriteAsync(api, plan, job.Settings.Operation, (count, detail) =>
            {
                activity.Enqueue($"{count}/{total} {detail}");
                if (activity.Count > 100) activity.Dequeue();
                progress?.Invoke(count, total, detail);
            }, cancellation, retired);
            Write(rulesFile, current.Select(e => e.ToString(SaveOptions.DisableFormatting)).ToArray());
            message = $"Completed {confirmed} item(s).";
            success = true;
        }
        catch (OperationCanceledException)
        {
            message = "Canceled or timed out. Verify any in-flight write.";
            success = false;
        }
        catch (Exception ex) { message = ex.Message; success = false; }
        var result = new ImportResult(started, DateTimeOffset.Now, message, activity.ToArray());
        Write(Location(id, "history"), new[] { result }.Concat(History(id)).Take(20).ToArray());
        progress?.Invoke(-1, 0, message);
        return success;
    }

    public static ImportSettings CurrentConnection(ScheduledImport job)
    {
        var settings = job.Settings;
        var saved = ConnectionStore.LoadUpdatedAfter(File.GetLastWriteTimeUtc(Location(job.Id, "job")));
        if (saved == null || !string.Equals(settings.Username, saved.Username, StringComparison.Ordinal) ||
            SophosApi.Endpoint(saved.Origin) != settings.Endpoint)
            return settings;
        return settings with { Password = saved.Password, CertificateFingerprint = saved.CertificateFingerprint };
    }

    private static string TaskName(Guid id) => $"Sophos Firewall Object Manager {id:N}";

    private static async Task RegisterAsync(ScheduledImport job)
    {
        XNamespace ns = "http://schemas.microsoft.com/windows/2004/02/mit/task";
        XElement Element(string name, params object[] content) => new(ns + name, content);
        using var identity = WindowsIdentity.GetCurrent();
        var sid = identity.User?.Value ?? throw new InvalidOperationException("The current Windows account has no SID.");
        var executable = Environment.ProcessPath ?? throw new InvalidOperationException("The application path is unavailable.");
        if (Path.GetFileName(executable).Equals("dotnet.exe", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Run the Windows application executable to register scheduled imports.");
        var xml = Element("Task", new XAttribute("version", "1.2"),
            Element("RegistrationInfo", Element("Description", "Refresh Sophos Firewall objects from a file or HTTPS source.")),
            Element("Triggers", Element("TimeTrigger",
                Element("Repetition", Element("Interval", $"PT{job.Minutes}M")),
                Element("StartBoundary", (job.StartAt ?? DateTime.Now.AddMinutes(job.Minutes)).ToString("yyyy-MM-ddTHH:mm:ss",
                    System.Globalization.CultureInfo.InvariantCulture)), Element("Enabled", true))),
            Element("Principals", Element("Principal", new XAttribute("id", "Author"),
                Element("UserId", sid), Element("LogonType", "InteractiveToken"), Element("RunLevel", "LeastPrivilege"))),
            Element("Settings", Element("MultipleInstancesPolicy", "IgnoreNew"),
                Element("DisallowStartIfOnBatteries", false), Element("StopIfGoingOnBatteries", false),
                Element("StartWhenAvailable", true), Element("Enabled", job.Enabled), Element("ExecutionTimeLimit", "PT0S")),
            Element("Actions", new XAttribute("Context", "Author"), Element("Exec",
                Element("Command", executable), Element("Arguments", $"--run-job {job.Id:N}"))));
        var temp = Location(job.Id, "task.xml");
        try
        {
            using (var writer = System.Xml.XmlWriter.Create(temp, new System.Xml.XmlWriterSettings
            {
                Encoding = System.Text.Encoding.Unicode,
                Indent = true
            }))
                xml.Save(writer);
            await SchedulerAsync("/create", "/tn", TaskName(job.Id), "/xml", temp, "/f");
        }
        finally { File.Delete(temp); }
    }

    private static async Task SchedulerAsync(params string[] args)
    {
        var start = new ProcessStartInfo(Path.Combine(Environment.SystemDirectory, "schtasks.exe"))
        {
            UseShellExecute = false, CreateNoWindow = true, RedirectStandardError = true, RedirectStandardOutput = true
        };
        foreach (var argument in args) start.ArgumentList.Add(argument);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start Windows Task Scheduler.");
        var errorTask = process.StandardError.ReadToEndAsync();
        var outputTask = process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"Windows Task Scheduler: {(await errorTask).Trim()} {(await outputTask).Trim()}");
    }

    internal static T Read<T>(string path)
    {
        byte[] encrypted;
        using (var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete))
        {
            encrypted = new byte[checked((int)file.Length)];
            file.ReadExactly(encrypted);
        }
        var bytes = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
        try { return JsonSerializer.Deserialize<T>(bytes) ?? throw new InvalidDataException("The task file is empty."); }
        finally { CryptographicOperations.ZeroMemory(bytes); }
    }

    internal static void Write<T>(string path, T item)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(item);
        var temporaryPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllBytes(temporaryPath, ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser));
            File.Move(temporaryPath, path, true);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(bytes);
            File.Delete(temporaryPath);
        }
    }
}

