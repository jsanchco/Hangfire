using HangFire.Application.Models;
using HangFire.Application.Services;

namespace HangFire.CodereJobs.BackgroundJobs1
{
    public class Service : CodereJobService
    {
        public Service(CodereJobConfig codereJobConfig) : base(codereJobConfig)
        {
        }

        public override Result Execute()
        {
            var result = new Result
            {
                Success = true,
                Start = DateTime.Now                
            };

            var execution = "In HangFire.CodereJobs.BackgroundJobs1.Execute ...";
            Console.WriteLine(execution);
            result.Summary.Add(execution);

            Thread.Sleep(1000);

            execution = "Executed OK";
            Console.WriteLine(execution);
            result.Summary.Add(execution);

            result.End = DateTime.Now;

            return result;
        }
    }
}
