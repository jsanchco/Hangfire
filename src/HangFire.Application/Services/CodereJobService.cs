using HangFire.Application.Config;
using HangFire.Application.Models;
using Serilog;

namespace HangFire.Application.Services
{
    public abstract class CodereJobService : ICodereJobService
    {
        private readonly CodereJobConfig _codereJobConfig;

        protected StatusJob _statusJob;
        protected readonly ILogger _logger;

        public string Name => _codereJobConfig.Name;
        public string Description => _codereJobConfig.Description;
        public string CronExpression => _codereJobConfig.CronExpression;
        public bool Enabled => _codereJobConfig.Enabled;
        public string StatusJob => _statusJob.ToString();

        public CodereJobService(CodereJobConfig codereJobConfig)
        {
            _codereJobConfig = codereJobConfig;
            _logger = new LoggerConfiguration()
                .WriteTo.File(
                    path: codereJobConfig.RouteLog,
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        public override string ToString()
        {
            return $"CodereJobService -> Name: [{Name}], Description: [{Description}], CronExpression: [{CronExpression}], Enabled: [{Enabled}], StatusJob: [{StatusJob}]";
        }

        public abstract Result Execute();
    }
}
