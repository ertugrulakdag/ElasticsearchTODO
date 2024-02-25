namespace ElasticsearchTODOWebApi.Models
{
    public class ConfigurationModel
    {
        public required string Host { get; set; }
        public int Port { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public string? FingerPrint { get; set; }
    }
}
