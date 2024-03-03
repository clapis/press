using MediatR;
using Quartz;

namespace Press.Hosts.WebAPI.Jobs;

[DisallowConcurrentExecution]
public class MediatrJob<TRequest>(IMediator mediator) : IJob where TRequest : new()
{
    public async Task Execute(IJobExecutionContext context) 
        => await mediator.Send(new TRequest(), context.CancellationToken);
}