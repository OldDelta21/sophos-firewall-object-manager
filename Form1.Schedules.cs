namespace SOPHOS_Host___Addresses_Manager;

public partial class Form1
{
    private ScheduledImport? SelectedTask => taskListBox.SelectedItem as ScheduledImport;

    private void RefreshTaskList(Guid? select = null)
    {
        try
        {
            select ??= SelectedTask?.Id;
            var tasks = ScheduleStore.List();
            taskListBox.Items.Clear();
            taskListBox.Items.AddRange(tasks);
            taskListBox.SelectedItem = tasks.FirstOrDefault(task => task.Id == select);
        }
        catch (Exception ex) { Log($"Could not read scheduled imports. {ex.Message}"); }
    }

    private void TaskSelectionChanged(object? sender, EventArgs e)
    {
        if (SelectedTask is not { } task)
        {
            taskNameBox.Clear();
            intervalBox.Value = 60;
            startTimeBox.Value = DateTime.Now.AddHours(1);
            taskEnabledBox.Checked = true;
            taskDetailsBox.Clear();
            return;
        }
        taskNameBox.Text = task.Name;
        intervalBox.Value = task.Minutes;
        startTimeBox.Value = task.StartAt ?? DateTime.Now.AddMinutes(task.Minutes);
        taskEnabledBox.Checked = task.Enabled;
        var settings = task.Settings;
        var details = new List<string>
        {
            $"{settings.Endpoint.Authority} · {settings.Username} · SFOS docs {settings.Family}",
            $"{settings.EffectiveSources.Length} source(s) · {settings.Operation} · {settings.Prefix}",
            $"{(settings.IpList ? "IPv4 address list" : "Individual entities")} · {settings.RuleAction} · {settings.SourceZone} → {settings.DestinationZone}",
            task.Enabled ? $"Scheduled every {task.Minutes} minutes while signed in." : "Schedule paused."
        };
        details.Add(task.StartAt is { } start ? $"First run: {start:g} (local time)." : "First run was set by an earlier build.");
        details.AddRange(settings.EffectiveSources.Select(source => source.ToString()));
        try
        {
            var history = ScheduleStore.History(task.Id);
            foreach (var run in history)
            {
                details.Add("");
                details.Add($"{run.Started.LocalDateTime:g} – {run.Finished.LocalDateTime:T} · {run.Message}");
                details.AddRange(run.Activity);
            }
        }
        catch (Exception ex) { details.Add($"Could not read run history. {ex.Message}"); }
        taskDetailsBox.Lines = details.ToArray();
    }

    private void NewTaskClick(object? sender, EventArgs e)
    {
        taskListBox.ClearSelected();
        TaskSelectionChanged(null, EventArgs.Empty);
        taskNameBox.Focus();
    }

    private void RefreshTasksClick(object? sender, EventArgs e) => RefreshTaskList();

    private async void ToggleTaskClick(object? sender, EventArgs e)
    {
        if (SelectedTask is not { } task) return;
        await RunAsync(async _ =>
        {
            await ScheduleStore.SetEnabledAsync(task.Id, !task.Enabled);
            RefreshTaskList(task.Id);
            Log(task.Enabled ? $"Paused “{task.Name}”." : $"Resumed “{task.Name}”.");
        });
    }

    private void LoadTaskClick(object? sender, EventArgs e)
    {
        if (SelectedTask is not { } task) return;
        ImportSettings settings;
        try { settings = ScheduleStore.CurrentConnection(task); }
        catch (Exception ex) { Log($"Could not load current connection settings. {ex.Message}"); return; }
        endpointBox.Text = settings.Origin;
        userBox.Text = settings.Username;
        passwordBox.Text = settings.Password;
        versionBox.Text = settings.ApiOverride;
        fingerprintBox.Text = settings.CertificateFingerprint ?? "";
        familyBox.Text = settings.Family;
        modeBox.SelectedItem = settings.Format;
        sourceBox.SelectedItem = settings.Source;
        pathBox.Text = settings.Path;
        inputBox.Text = settings.PastedInput;
        prefixBox.Text = settings.Prefix;
        operationBox.SelectedIndex = settings.Operation switch { "add" => 1, "update" => 2, _ => 0 };
        addressKindBox.SelectedIndex = settings.IpList ? 1 : 0;
        actionBox.SelectedItem = settings.RuleAction;
        sourceZoneBox.Text = settings.SourceZone;
        destinationZoneBox.Text = settings.DestinationZone;
        directionBox.SelectedItem = settings.MatchSource ? "Source" : "Destination";
        enableBox.Checked = settings.RuleEnabled;
        groupIpsBox.Checked = settings.GroupIps;
        groupFqdnsBox.Checked = settings.GroupFqdns;
        wildcardsBox.Checked = settings.Wildcards;
        reverseRuleBox.Checked = settings.ReverseRule;
        sourceListBox.Items.Clear();
        if (settings.Sources is { Length: > 0 }) sourceListBox.Items.AddRange(settings.Sources);
        UpdateSources();
        statusLabel.Text = $"Loaded {task.Name}. Changes take effect after saving.";
    }

    private async void SaveTaskClick(object? sender, EventArgs e) => await RunAsync(async cancellation =>
    {
        var name = taskNameBox.Text.Trim();
        if (name.Length == 0) throw new FormatException("Enter a name for the scheduled import.");
        var settings = Settings(true);
        var task = new ScheduledImport(SelectedTask?.Id ?? Guid.NewGuid(), name,
            (int)intervalBox.Value, taskEnabledBox.Checked, settings, DateTime.SpecifyKind(
                startTimeBox.Value.AddTicks(-(startTimeBox.Value.Ticks % TimeSpan.TicksPerMinute)), DateTimeKind.Unspecified));
        if (task.Enabled && MessageBox.Show(this,
            $"Save “{name}” to apply {settings.EffectiveSources.Length} source(s) to {settings.Endpoint.Authority} " +
            $"every {task.Minutes} minutes from {task.StartAt:g} local time while you are signed in?",
            Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK) return;
        var apiVersion = settings.Endpoint == negotiatedEndpoint ? negotiatedVersion : null;
        await settings.PlanAsync(apiVersion, cancellation);
        cancellation.ThrowIfCancellationRequested();
        await ScheduleStore.SaveAsync(task);
        RefreshTaskList(task.Id);
        Log($"Saved task “{task.Name}”.");
    });

    private async void RunTaskClick(object? sender, EventArgs e)
    {
        if (SelectedTask is not { } task) return;
        if (MessageBox.Show(this, $"Run “{task.Name}” now and apply changes to {task.Settings.Endpoint.Authority}?",
            Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK) return;
        await RunAsync(async cancellation =>
        {
            await ScheduleStore.RunAsync(task.Id, true, ShowProgress, cancellation);
            RefreshTaskList(task.Id);
        });
    }

    private async void DeleteTaskClick(object? sender, EventArgs e)
    {
        if (SelectedTask is not { } task) return;
        if (MessageBox.Show(this, $"Delete the schedule and run history for “{task.Name}”?",
            Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK) return;
        await RunAsync(async _ =>
        {
            await ScheduleStore.DeleteAsync(task.Id);
            RefreshTaskList();
            Log($"Deleted task “{task.Name}”.");
        });
    }
}

