using MediatR;
using Press.Core.Domain;
using Press.Core.Infrastructure;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.Notifications.Monitor;

public class MonitorHandler(
    IPublicationStore publicationStore, 
    INotificationService notificationService) 
    : IRequestHandler<MonitorRequest>
{
    private static readonly TimeSpan MaxPublicationAge = TimeSpan.FromDays(3);
    
    public async Task Handle(MonitorRequest request, CancellationToken cancellationToken)
    {
        var latest = await publicationStore.GetLatestPublicationsBySourceAsync(cancellationToken);

        var delayed = latest
            .Where(x => x.Source == PublicationSource.Sorocaba)
            .Where(x => DateTime.UtcNow - x.CreatedOn > MaxPublicationAge)
            .ToList();

        if (!delayed.Any()) return;

        await notificationService.SendMonitorAsync(delayed, cancellationToken);
    }
}