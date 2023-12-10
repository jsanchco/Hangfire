using HangFire.Application.Config;
using HangFire.Application.Models;

namespace HangFire.Application.Services
{
    public interface ICodereJobService
    {
        public string Name { get; }
        public string Description { get; }
        public string CronExpression { get; }
        public bool Enabled { get; }
        public string StatusJob { get; }

        Result Execute();
    }
}
