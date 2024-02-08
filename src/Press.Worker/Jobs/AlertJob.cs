using Press.Core.Alerts;
using Quartz;

namespace Press.Worker.Jobs
{
    [DisallowConcurrentExecution]
    public class AlertJob : IJob
    {
        private readonly AlertService _service;

        public AlertJob(AlertService service) => _service = service;

        public async Task Execute(IJobExecutionContext context) 
            => await _service.AlertAsync(context.CancellationToken);
    }
}