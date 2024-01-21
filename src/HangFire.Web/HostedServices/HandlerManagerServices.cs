using Hangfire;
using HangFire.Application.Models;
using HangFire.Application.Services;
using System.Reflection;

namespace HangFire.Web.HostedServices
{
    public class HandlerManagerServices : IHostedService
    {
        private readonly ILogger<HandlerManagerServices> _logger;
        private readonly IConfiguration _configuration;
        private readonly List<CodereJobService> _jobServices = new();
        private bool _available;

        public HandlerManagerServices(
            ILogger<HandlerManagerServices> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public bool Available => _available;

        public List<CodereJobService> JobServices => _jobServices;

        public Task StartAsync(CancellationToken stoppingToken)
        {
            try
            {
                ReloadServices(stoppingToken);
                _available = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in loading CodereJobsConfig");
                _available = false;
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void ReloadServices(CancellationToken stoppingToken = default)
        {
            var codereJobsConfig = _configuration
                .GetSection("CodereJobsConfig")
                .Get<List<CodereJobConfig>>();

            if (codereJobsConfig == null || !codereJobsConfig.Any())
                return;

            _jobServices.Clear();
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var jobConfig in codereJobsConfig)
                {
                    var type = GetTypeFromConfig(jobConfig);
                    if (type is null)
                        continue;

                    RecurringJob.AddOrUpdate(
                        jobConfig.Name,
                        () => ExecuteJob(type, jobConfig),
                        jobConfig.CronExpression);
                }
                return;
            }
        }

        private void ExecuteJob(Type type, CodereJobConfig jobConfig)
        {
            var instance = Activator.CreateInstance(type, new object[] { jobConfig });
            var methodInfo = type.GetMethod("Execute");
            methodInfo?.Invoke(instance, null);
        }

        public CodereJobService? GetCodereJobServiceByName(string name)
        {
            return _jobServices.FirstOrDefault(x => x.Name == name);
        }

        public IEnumerable<CodereJobService> GetCodereJobServicesEnabled()
        {
            return _jobServices.Where(x => x.Enabled);
        }

        private Type? GetTypeFromConfig(CodereJobConfig jobConfig)
        {
            var assembly = Assembly.LoadFrom(jobConfig.RouteAssembly);
            if (assembly == null)
            {
                _logger.LogInformation($"Assembly [{jobConfig.RouteAssembly}] NOT found");
                return null;
            }

            var types = assembly.GetTypes();
            var findType = types.FirstOrDefault(x => x.FullName == jobConfig.NameSpace);
            if (findType == null)
            {
                _logger.LogInformation($"Type [{jobConfig.NameSpace}] NOT found in Assembly [{assembly.FullName}]");
                return null;
            }

            if (findType.BaseType?.Name != "CodereJobService")
            {
                _logger.LogInformation($"Type [CodereJobService] NOT found in Assembly [{assembly.FullName}]");
                return null;
            }

            var methodInfo = findType.GetMethod("Execute");
            if (methodInfo == null)
            {
                _logger.LogInformation($"NOT possible find method 'Execute' in [Assembly: {jobConfig.RouteAssembly}], [NameSpace: {jobConfig.NameSpace}]");
                return null;
            }

            return findType;
        }
    }
}
