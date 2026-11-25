using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SOPHOS_Host___Addresses_Manager;

public partial class Form1 : Form
{
    private sealed record Preview(ImportSettings Settings, XElement[] Entities);
    private Preview? preview;
    private CancellationTokenSource? active;
    private string? negotiatedVersion;
    private Uri? negotiatedEndpoint;

    public Form1()
    {
        InitializeComponent();
        startTimeBox.Value = DateTime.Now.AddHours(1);
        templateBox.Items.AddRange(AddressFeeds.Templates.Select(template => (object)template.Title).ToArray());
        templateBox.SelectedIndex = 0;
        UpdateSourceEditor(null, EventArgs.Empty);
        foreach (var box in new[] { endpointBox, userBox, passwordBox, versionBox, fingerprintBox, prefixBox, pathBox,
                     sourceZoneBox, destinationZoneBox, inputBox })
            box.TextChanged += InvalidatePreview;
        foreach (var box in new[] { familyBox, modeBox, sourceBox, operationBox, directionBox, actionBox, addressKindBox })
            box.TextChanged += InvalidatePreview;
        enableBox.CheckedChanged += InvalidatePreview;
        groupIpsBox.CheckedChanged += InvalidatePreview;
        groupFqdnsBox.CheckedChanged += InvalidatePreview;
        wildcardsBox.CheckedChanged += InvalidatePreview;
        reverseRuleBox.CheckedChanged += InvalidatePreview;
        modeBox.SelectedIndexChanged += UpdateHostOptions;
        addressKindBox.SelectedIndexChanged += UpdateHostOptions;
        UpdateHostOptions(null, EventArgs.Empty);
        Shown += (_, _) =>
        {
            RestoreConnection();
            RefreshTaskList();
            trayIcon.Visible = true;
        };
    }

    private void UpdateHostOptions(object? sender, EventArgs e)
    {
        var hasAddresses = sourceListBox.Items.Count == 0 ? modeBox.Text != "Entity XML / JSON" :
            sourceListBox.Items.OfType<ImportSource>().Any(source => source.Format != "Entity XML / JSON");
        var individualHosts = addressKindBox.SelectedIndex == 0 && hasAddresses;
        groupIpsBox.Enabled = groupFqdnsBox.Enabled = wildcardsBox.Enabled = individualHosts;
    }

    private void InvalidatePreview(object? sender, EventArgs e)
    {
        if (preview != null) previewBox.Clear();
        preview = null;
        applyButton.Enabled = false;
    }

    private SavedConnection ConnectionSettings(bool requireAccount)
    {
        _ = SophosApi.Endpoint(endpointBox.Text);
        if (requireAccount && (string.IsNullOrWhiteSpace(userBox.Text) || passwordBox.Text.Length == 0))
            throw new FormatException("Enter the firewall username and password.");
        var version = versionBox.Text.Trim();
        if (version.Length != 0 && !Regex.IsMatch(version, @"\A[0-9]+(?:\.[0-9]+)+\z"))
            throw new FormatException("Enter an XML API version such as 2300.1, or leave the override blank.");
        return new SavedConnection(endpointBox.Text.Trim(), userBox.Text.Trim(), passwordBox.Text, version, familyBox.Text,
            SophosApi.NormalizeFingerprint(fingerprintBox.Text));
    }

    private ImportSettings Settings(bool requireAccount)
    {
        var account = ConnectionSettings(requireAccount);
        var sources = sourceListBox.Items.OfType<ImportSource>().ToArray();
        var source = sources.FirstOrDefault();
        if (source == null)
        {
            var value = sourceBox.Text == "Paste" ? inputBox.Text : pathBox.Text.Trim();
            if (sourceBox.Text == "File" && value.Length != 0) value = Path.GetFullPath(value);
            source = new ImportSource(sourceBox.Text, modeBox.Text, value);
        }
        return new ImportSettings(account.Origin, account.Username, account.Password, account.ApiOverride,
            account.Family, source.Format, source.Source, source.Source == "Paste" ? "" : source.Value,
            source.Source == "Paste" ? source.Value : "", prefixBox.Text.Trim(),
            operationBox.SelectedIndex switch { 1 => "add", 2 => "update", _ => "upsert" },
            addressKindBox.SelectedIndex == 1, actionBox.Text, sourceZoneBox.Text.Trim(),
            destinationZoneBox.Text.Trim(), directionBox.Text == "Source", enableBox.Checked, account.CertificateFingerprint,
            groupIpsBox.Checked, groupFqdnsBox.Checked, wildcardsBox.Checked, reverseRuleBox.Checked,
            sources);
    }

    private async void ConnectClick(object? sender, EventArgs e) => await RunAsync(async cancellation =>
    {
        var settings = ConnectionSettings(true);
        var endpoint = SophosApi.Endpoint(settings.Origin);
        var (_, response) = await AuthenticateWithApprovalAsync(settings, endpoint, cancellation);
        negotiatedEndpoint = endpoint;
        negotiatedVersion = (string?)response.Attribute("APIVersion");
        Log(negotiatedVersion == null ? "Firewall authentication succeeded." :
            $"Firewall authentication succeeded. XML API version {negotiatedVersion}.");
    });

    private async Task<(SavedConnection Settings, XElement Response)> AuthenticateWithApprovalAsync(
        SavedConnection settings, Uri endpoint, CancellationToken cancellation)
    {
        XElement response;
        try { response = await AuthenticateAsync(settings, endpoint, cancellation); }
        catch (FirewallTlsException failure)
        {
            var approved = await RequestCertificateApprovalAsync(endpoint, settings.CertificateFingerprint, failure, cancellation);
            if (approved == null) throw;
            fingerprintBox.Text = approved;
            settings = settings with { CertificateFingerprint = approved };
            response = await AuthenticateAsync(settings, endpoint, cancellation);
            ConnectionStore.Save(settings);
        }
        return (settings, response);
    }

    private static async Task<XElement> AuthenticateAsync(SavedConnection settings, Uri endpoint, CancellationToken cancellation)
    {
        using var nativeApi = new SophosApi(endpoint, settings.Username, settings.Password, "",
            certificateFingerprint: settings.CertificateFingerprint);
        var response = await nativeApi.SendAsync(null, cancellation);
        if (settings.ApiOverride.Length != 0)
        {
            using var explicitApi = new SophosApi(endpoint, settings.Username, settings.Password, settings.ApiOverride,
                certificateFingerprint: settings.CertificateFingerprint);
            await explicitApi.SendAsync(null, cancellation);
        }
        return response;
    }

    private async Task<string?> RequestCertificateApprovalAsync(Uri endpoint, string? previous,
        FirewallTlsException failure, CancellationToken cancellation)
    {
        PresentedCertificate? certificate;
        try { certificate = await SophosApi.InspectCertificateAsync(endpoint, cancellation); }
        catch (Exception ex) when (!cancellation.IsCancellationRequested)
        {
            throw new InvalidOperationException($"{failure.Message}{Environment.NewLine}Certificate inspection failed. {ex.Message}", failure);
        }
        cancellation.ThrowIfCancellationRequested();
        if (certificate == null || string.Equals(previous, certificate.Fingerprint, StringComparison.OrdinalIgnoreCase))
            return null;
        var action = string.IsNullOrEmpty(previous) ? "Approve this certificate" : "Replace the previous certificate approval";
        var prior = string.IsNullOrEmpty(previous) ? "" : $"\nPrevious SHA-256: {previous}\n";
        var result = MessageBox.Show(this,
            $"Firewall: {endpoint.Authority}\nSubject: {certificate.Subject}\nIssuer: {certificate.Issuer}\n" +
            $"Expires: {certificate.Expires:g}\nWindows validation: {certificate.Errors}\n\n" +
            $"Presented SHA-256:\n{certificate.Fingerprint}\n{prior}\n" +
            "Compare the fingerprint with the firewall administrator or a trusted certificate export. " +
            "Approval allows the app to send the saved credentials to a server presenting this certificate.\n\n" +
            $"{action} and retry the connection?",
            "Approve firewall certificate", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
        return result == DialogResult.Yes ? certificate.Fingerprint : null;
    }

    private void DocsClick(object? sender, EventArgs e)
    {
        if (!Regex.IsMatch(familyBox.Text, @"\A(?:19|[2-9][0-9])\.[0-9]+\z"))
        {
            MessageBox.Show(this, "Enter the SFOS release family, for example 23.0.", Text);
            return;
        }
        try
        {
            var url = familyBox.Text == "19.5"
                ? "https://techvids.sophos.com/watch/7oEPqyoKz7ZSPn27Pze8hu"
                : $"https://docs.sophos.com/nsg/sophos-firewall/{familyBox.Text}/api/index.html";
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (System.ComponentModel.Win32Exception ex) { Log(ex.Message); }
    }

    private void BrowseClick(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog { Filter = "Import files|*.txt;*.csv;*.json;*.xml|All files|*.*", Multiselect = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        sourceBox.SelectedItem = "File";
        pathBox.Text = dialog.FileName;
        modeBox.SelectedItem = FileFormat(dialog.FileName);
        if (dialog.FileNames.Length > 1 || sourceListBox.Items.Count > 0)
        {
            foreach (var file in dialog.FileNames)
                AddSource(new ImportSource("File", FileFormat(file), file, CurrentSourcePrefix(FileFormat(file))));
            tabs.SelectedTab = sourcesPage;
        }
    }

    private static string FileFormat(string file) => Path.GetExtension(file).ToLowerInvariant() switch
    {
        ".xml" => "Entity XML / JSON", ".json" => "JSON", ".csv" => "CSV", _ => "TXT"
    };

    private void AddSourceClick(object? sender, EventArgs e)
    {
        try
        {
            var kind = sourceBox.Text;
            var value = kind == "Paste" ? inputBox.Text : pathBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(value)) throw new FormatException("Enter or paste a source before adding it.");
            if (kind == "File") value = Path.GetFullPath(value);
            AddSource(new ImportSource(kind, modeBox.Text, value, CurrentSourcePrefix(modeBox.Text)));
            tabs.SelectedTab = sourcesPage;
        }
        catch (Exception ex) { Log(ex.Message); }
    }

    private string? CurrentSourcePrefix(string format) =>
        format == "Entity XML / JSON" || string.IsNullOrWhiteSpace(prefixBox.Text) ? null : prefixBox.Text.Trim();

    private void AddSource(ImportSource source)
    {
        if (!sourceListBox.Items.Contains(source)) sourceListBox.Items.Add(source);
        sourceListBox.ClearSelected();
        sourceListBox.SelectedItem = source;
        UpdateSources();
    }

    private void RemoveSourceClick(object? sender, EventArgs e)
    {
        foreach (var source in sourceListBox.SelectedItems.Cast<ImportSource>().ToArray())
            sourceListBox.Items.Remove(source);
        if (sourceListBox.Items.Count == 0) ClearSourceFields();
        UpdateSources();
    }

    private void ClearSourcesClick(object? sender, EventArgs e)
    {
        sourceListBox.Items.Clear();
        ClearSourceFields();
        UpdateSources();
    }

    private void ClearSourceFields()
    {
        pathBox.Clear();
        inputBox.Clear();
    }

    private void AddTemplateClick(object? sender, EventArgs e)
    {
        if (templateBox.SelectedIndex < 0) return;
        var template = AddressFeeds.Templates[templateBox.SelectedIndex];
        AddSource(new ImportSource("HTTPS URL", "TXT", template.Url, template.Prefix));
    }

    private void UpdateSourceEditor(object? sender, EventArgs e)
    {
        var source = sourceListBox.SelectedItems.Count == 1 ? sourceListBox.SelectedItem as ImportSource : null;
        var editable = source != null && source.Format != "Entity XML / JSON";
        sourceNameBox.Enabled = sourceLimitBox.Enabled = applySourceOptionsButton.Enabled = editable;
        sourceNameBox.Text = editable ? source!.Prefix ?? "" : "";
        sourceLimitBox.Value = editable ? source!.MaxEntries ?? 0 : 0;
        removeSourceButton.Enabled = sourceListBox.SelectedItems.Count > 0;
        clearSourcesButton.Enabled = sourceListBox.Items.Count > 0;
    }

    private void ApplySourceOptionsClick(object? sender, EventArgs e)
    {
        if (sourceListBox.SelectedIndices.Count != 1 || sourceListBox.SelectedItem is not ImportSource source) return;
        var prefix = sourceNameBox.Text.Trim();
        var index = sourceListBox.SelectedIndex;
        sourceListBox.Items[index] = source with
        {
            Prefix = prefix.Length == 0 ? null : prefix,
            MaxEntries = sourceLimitBox.Value == 0 ? null : (int)sourceLimitBox.Value
        };
        sourceListBox.SelectedIndex = index;
        UpdateSources();
    }

    private void UpdateSources()
    {
        sourcesPage.Text = $"Import sources ({sourceListBox.Items.Count})";
        InvalidatePreview(null, EventArgs.Empty);
        UpdateHostOptions(null, EventArgs.Empty);
        UpdateSourceEditor(null, EventArgs.Empty);
    }

    private async void PreviewClick(object? sender, EventArgs e) => await RunAsync(async cancellation =>
    {
        InvalidatePreview(null, EventArgs.Empty);
        var settings = Settings(false);
        var apiVersion = settings.Endpoint == negotiatedEndpoint ? negotiatedVersion : null;
        var entities = await settings.PlanAsync(apiVersion, cancellation);
        previewBox.Text = $"{settings.Operation.ToUpperInvariant()} · {entities.Length} API write(s) · {settings.Endpoint.Authority}\r\n\r\n" +
            new XElement("Entities", entities);
        preview = new Preview(settings, entities);
        tabs.SelectedTab = previewPage;
        statusLabel.Text = $"Preview ready. {entities.Length} API write(s).";
    });

    private async void ApplyClick(object? sender, EventArgs e)
    {
        if (preview is not { } plan) return;
        if (MessageBox.Show(this, $"Apply {plan.Entities.Length} write(s) to {plan.Settings.Endpoint.Authority}?\n\n" +
            "The app stops at the first unconfirmed write. Earlier confirmed changes remain applied.",
            Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK) return;

        await RunAsync(async cancellation =>
        {
            var connectionSettings = ConnectionSettings(true);
            using var target = ImportRunner.LockEndpoint(plan.Settings.Endpoint);
            var (authenticated, response) = await AuthenticateWithApprovalAsync(
                connectionSettings, plan.Settings.Endpoint, cancellation);
            var settings = plan.Settings with { CertificateFingerprint = authenticated.CertificateFingerprint };
            using var api = settings.Api();
            TransferPlan.Validate(plan.Entities, plan.Settings.Operation, (string?)response.Attribute("APIVersion"));
            InvalidatePreview(null, EventArgs.Empty);
            tabs.SelectedTab = activityPage;
            ShowProgress(0, plan.Entities.Length, "Sending changes to the firewall…");
            var completed = await ImportRunner.WriteAsync(api, plan.Entities, plan.Settings.Operation, (count, message) =>
                ShowProgress(count, plan.Entities.Length, message), cancellation);
            Log($"Completed {completed} item(s). Activity identifies confirmed writes and skipped items.");
        });
    }

    private async void ExportClick(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Entity XML|*.xml|Entity JSON|*.json", AddExtension = true, OverwritePrompt = true
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        await RunAsync(async cancellation =>
        {
            var type = TransferPlan.EntityTag(entityBox.Text.Trim());
            using var api = Settings(true).Api();
            var result = await api.SendAsync(new XElement("Get", new XElement(type)), cancellation);
            var entities = result.Elements(type).Where(entity => entity.HasElements && entity.Elements()
                .Any(child => child.Name != "Status" || child.Attribute("code") == null)).Select(entity =>
            {
                var copy = new XElement(entity);
                copy.Attribute("transactionid")?.Remove();
                copy.Elements("Status").Where(e => e.Attribute("code") != null).Remove();
                return copy;
            }).ToArray();
            if (entities.Length == 0)
            {
                Log($"No {type} entities returned. Check that this tag supports Get on the target release.");
                return;
            }

            var body = dialog.FilterIndex == 2
                ? JsonSerializer.Serialize(new { entities = entities.Select(x => x.ToString()) },
                    new JsonSerializerOptions { WriteIndented = true })
                : new XElement("Entities", entities).ToString();
            var temporaryPath = dialog.FileName + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                await File.WriteAllTextAsync(temporaryPath, body, cancellation);
                cancellation.ThrowIfCancellationRequested();
                File.Move(temporaryPath, dialog.FileName, true);
            }
            finally { File.Delete(temporaryPath); }
            Log($"Exported {entities.Length} {type} entities to {dialog.FileName}.");
        });
    }

    private async Task RunAsync(Func<CancellationToken, Task> work)
    {
        if (active != null) return;
        using var cancellation = new CancellationTokenSource();
        active = cancellation;
        connection.Enabled = transfer.Enabled = blacklist.Enabled = inputPage.Enabled = schedulePage.Enabled = sourcesPage.Enabled = false;
        saveConnectionButton.Enabled = forgetConnectionButton.Enabled = false;
        progressBar.Value = 0;
        progressBar.Style = ProgressBarStyle.Marquee;
        cancelButton.Enabled = true;
        statusLabel.Text = "Working…";
        try
        {
            await work(cancellation.Token);
            if (progressBar.Style == ProgressBarStyle.Marquee)
            {
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Value = progressBar.Maximum;
            }
        }
        catch (OperationCanceledException)
        {
            Log("Operation canceled. If a write was in flight, check the firewall before retrying.");
            tabs.SelectedTab = activityPage;
        }
        catch (Exception ex)
        {
            Log(ex.Message);
            tabs.SelectedTab = activityPage;
        }
        finally
        {
            active = null;
            if (progressBar.Style == ProgressBarStyle.Marquee)
                progressBar.Style = ProgressBarStyle.Continuous;
            connection.Enabled = transfer.Enabled = blacklist.Enabled = inputPage.Enabled = schedulePage.Enabled = sourcesPage.Enabled = true;
            saveConnectionButton.Enabled = forgetConnectionButton.Enabled = true;
            cancelButton.Enabled = false;
            applyButton.Enabled = preview != null;
            if (exitRequested) BeginInvoke(Close);
        }
    }

    private void ShowProgress(int count, int total, string message)
    {
        if (count < 0)
        {
            if (progressBar.Style == ProgressBarStyle.Marquee) progressBar.Style = ProgressBarStyle.Continuous;
            Log(message);
            return;
        }
        progressBar.Style = ProgressBarStyle.Continuous;
        progressBar.Maximum = Math.Max(total, 1);
        progressBar.Value = count;
        if (count == 0) statusLabel.Text = $"0/{total} · {message}";
        else Log($"{count}/{total} · {message}");
    }

    private void CancelClick(object? sender, EventArgs e) => active?.Cancel();

    private void Log(string message)
    {
        activityBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        statusLabel.Text = message;
    }
}
