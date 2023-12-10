namespace HangFire.Application.Models
{
    public class CodereJobConfig
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string CronExpression { get; set; }
        public bool Enabled { get; set; }
        public string NameSpace { get; set; }
        public string Assembly { get; set; }
        public string Folder { get; set; }
        public string RouteLog { get; set; }
        public string RouteAssembly => $"{Folder}\\{Assembly}";
    }
}
