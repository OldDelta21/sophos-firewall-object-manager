namespace SOPHOS_Host___Addresses_Manager;

public partial class Form1
{
    private bool exitRequested;

    private void RestoreConnection()
    {
        try
        {
            if (ConnectionStore.Load() is not { } saved) return;
            endpointBox.Text = saved.Origin;
            userBox.Text = saved.Username;
            passwordBox.Text = saved.Password;
            versionBox.Text = saved.ApiOverride;
            fingerprintBox.Text = saved.CertificateFingerprint ?? "";
            familyBox.Text = saved.Family;
        }
        catch (Exception ex) { Log($"Could not load the saved connection. {ex.Message}"); }
    }

    private void SaveConnectionClick(object? sender, EventArgs e)
    {
        try
        {
            ConnectionStore.Save(ConnectionSettings(true));
            Log("Connection saved for this Windows user.");
        }
        catch (Exception ex) { Log(ex.Message); }
    }

    private void ForgetConnectionClick(object? sender, EventArgs e)
    {
        try
        {
            ConnectionStore.Forget();
            passwordBox.Clear();
            fingerprintBox.Clear();
            Log("Saved connection removed. Scheduled imports retain their own credentials.");
        }
        catch (Exception ex) { Log(ex.Message); }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing && !exitRequested)
        {
            Hide();
            e.Cancel = true;
        }
        base.OnFormClosing(e);
        if (!e.Cancel) trayIcon.Visible = false;
    }

    private void OpenFromTray(object? sender, EventArgs e)
    {
        if (exitRequested) return;
        Show();
        if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
        Activate();
    }

    private void ExitFromTray(object? sender, EventArgs e)
    {
        exitRequested = true;
        if (active == null) Close();
        else
        {
            trayExitItem.Enabled = false;
            active.Cancel();
        }
    }
}
