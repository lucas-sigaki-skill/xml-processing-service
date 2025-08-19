namespace XmlProcessingService.Models
{
    public class ServiceConfiguration
    {
        public string WatchFolder { get; set; } = string.Empty;
        public string ProcessedFolder { get; set; } = string.Empty;
        public string ErrorFolder { get; set; } = string.Empty;
        public string DatabaseType { get; set; } = "MSSQL";
        public string ConnectionString { get; set; } = string.Empty;
        public string LogFilePath { get; set; } = string.Empty;
    }
}
