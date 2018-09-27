namespace Kongverge.DTOs
{
    public class Settings
    {
        public Admin Admin { get; set; } = new Admin();
        public string InputFolder { get; set; }
        public bool DryRun { get; set; } = false;
        public string OutputFolder { get; set; }
        public static string FileExtension { get; } = ".json";
        public static string GlobalConfigFileName { get; } = "global.json";
    }

    public class Admin
    {
        public string Host { get; set; }
        public int Port { get; set; }
    }
}
