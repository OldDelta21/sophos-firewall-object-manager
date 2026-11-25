namespace SOPHOS_Host___Addresses_Manager
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            if (args is ["--run-job", var jobId] && Guid.TryParseExact(jobId, "N", out var id))
            {
                try { return ScheduleStore.RunAsync(id, false, null, CancellationToken.None).GetAwaiter().GetResult() ? 0 : 1; }
                catch (Exception) { return 1; }
            }
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
            return 0;
        }
    }
}
