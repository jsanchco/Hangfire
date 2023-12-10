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
            catch(Exception ex)
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
                    AddJobService(jobConfig);
                }
                return;
            }
        }

        public CodereJobService? GetCodereJobServiceByName(string name)
        {
            return _jobServices.FirstOrDefault(x => x.Name == name);
        }

        public IEnumerable<CodereJobService> GetCodereJobServicesEnabled()
        {
            return _jobServices.Where(x => x.Enabled);
        }

        private object? AddJobService(CodereJobConfig jobConfig)
        {
            var typeService = GetTypeService(jobConfig.RouteAssembly, jobConfig.NameSpace);
            if (typeService == null)
            {
                _logger.LogInformation($"NOT possible GetTypeService [Assembly: {jobConfig.RouteAssembly}], [NameSpace: {jobConfig.NameSpace}]");
                return null;
            }

            var instance = Activator.CreateInstance(typeService, new object[] { jobConfig });
            if (instance == null)
            {
                _logger.LogInformation($"NOT possible CreateInstance [Assembly: {jobConfig.RouteAssembly}], [NameSpace: {jobConfig.NameSpace}]");
                return null;
            }

            var methodInfo = typeService.GetMethod("Execute");
            if (methodInfo == null)
            {
                _logger.LogInformation($"NOT possible find method 'Execute' in [Assembly: {jobConfig.RouteAssembly}], [NameSpace: {jobConfig.NameSpace}]");
                return null;
            }

            var codereJobService = (CodereJobService)instance;
            _jobServices.Add(codereJobService);
            _logger.LogInformation($"Add {codereJobService}");

            return instance;
        }

        private Type? GetTypeService(string assemblyFile, string serviceTypeOf)
        {
            var assembly = Assembly.LoadFrom(assemblyFile);
            if (assembly == null)
            {
                _logger.LogInformation($"Assembly [{assemblyFile}] NOT found");
                return null;
            }

            var types = assembly.GetTypes();
            var findType = types.FirstOrDefault(x => x.FullName == serviceTypeOf);
            if (findType == null)
            {
                _logger.LogInformation($"Type [{serviceTypeOf}] NOT found in Assembly [{assembly.FullName}]");
                return null;
            }

            if (findType.BaseType?.Name != "CodereJobService")
            {
                _logger.LogInformation($"Type [CodereJobService] NOT found in Assembly [{assembly.FullName}]");
                return null;
            }

            return findType;
        }
    }
}
