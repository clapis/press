using System.Threading.Tasks;
using Press.Core.Alerts;
using Quartz;

namespace Press.Worker.Jobs
{
    [DisallowConcurrentExecution]
    public class ReportJob : IJob
    {
        private readonly ReportService _service;

        public ReportJob(ReportService service) => _service = service;

        public async Task Execute(IJobExecutionContext context) 
            => await _service.ReportAsync(context.CancellationToken);
    }
}