using Press.Core.Features.Alerts.Alert;
using Press.Core.Features.Alerts.Report;
using Press.Core.Features.Sources.Scrape;
using Quartz;

namespace Press.Hosts.WebAPI.Jobs;

// TODO: abstract to avoid this unnecessary repetition.

[DisallowConcurrentExecution]
public class ScrapeJob(SourcesScrapeHandler handler) : IJob
{
    public async Task Execute(IJobExecutionContext context) 
        => await handler.HandleAsync(context.CancellationToken);
}

[DisallowConcurrentExecution]
public class AlertJob(AlertHandler handler) : IJob
{
    public async Task Execute(IJobExecutionContext context) 
        => await handler.HandleAsync(context.CancellationToken);
}

[DisallowConcurrentExecution]
public class ReportJob(ReportHandler handler) : IJob
{
    public async Task Execute(IJobExecutionContext context) 
        => await handler.HandleAsync(context.CancellationToken);
}


