using Press.Core.Publications;
using Quartz;

namespace Press.Worker.Jobs
{
    [DisallowConcurrentExecution]
    public class SyncJob : IJob
    {
        private readonly SynchronizationService _service;

        public SyncJob(SynchronizationService service) => _service = service;

        public async Task Execute(IJobExecutionContext context) 
            => await _service.SynchronizeAsync(context.CancellationToken);
    }
}