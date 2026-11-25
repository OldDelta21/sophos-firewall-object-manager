namespace SOPHOS_Host___Addresses_Manager;

internal sealed record SavedConnection(string Origin, string Username, string Password, string ApiOverride, string Family,
    string? CertificateFingerprint = null);

internal static class ConnectionStore
{
    private static readonly string PathName = Path.Combine(ScheduleStore.DataDirectory, "connection.profile");

    public static SavedConnection? Load() => File.Exists(PathName) ? ScheduleStore.Read<SavedConnection>(PathName) : null;

    public static SavedConnection? LoadUpdatedAfter(DateTime cutoffUtc) =>
        File.GetLastWriteTimeUtc(PathName) > cutoffUtc ? Load() : null;

    public static void Save(SavedConnection connection)
    {
        Directory.CreateDirectory(ScheduleStore.DataDirectory);
        ScheduleStore.Write(PathName, connection);
    }

    public static void Forget() => File.Delete(PathName);
}
