using MediatR;
using Press.Core.Infrastructure;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.System.MonitorDelay;

public record MonitorDelayRequest() : IRequest;

public class MonitorDelayHandler(
    IPublicationStore publicationStore, 
    INotificationService notificationService) 
    : IRequestHandler<MonitorDelayRequest>
{
    private static readonly TimeSpan MaxPublicationAge = TimeSpan.FromDays(3);
    
    public async Task Handle(MonitorDelayRequest delayRequest, CancellationToken cancellationToken)
    {
        var latest = await publicationStore.GetLatestBySourceAsync(cancellationToken);

        var delayed = latest
            .Where(x => DateTime.UtcNow - x.CreatedOn > MaxPublicationAge)
            .ToList();

        if (!delayed.Any()) return;

        await notificationService.SendDelayNotificationAsync(delayed, cancellationToken);
    }
}