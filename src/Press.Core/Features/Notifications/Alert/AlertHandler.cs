using MediatR;
using Press.Core.Infrastructure;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.Notifications.Alert;

public class AlertHandler(
    IAlertStore alertStore,
    IPublicationStore publicationStore,
    INotificationService notificationService)
    : IRequestHandler<AlertRequest>
{
    public async Task Handle(AlertRequest request, CancellationToken cancellationToken)
    {
        var alerts = await alertStore.GetAllAsync(cancellationToken);

        foreach (var alert in alerts)
        {
            var pubs = await publicationStore.SearchAsync(alert.Term, cancellationToken);

            var last = alert.LastNotification ?? DateTime.MinValue;

            var notifications = pubs.Where(p => p.CreatedOn > last).ToList();

            if (!notifications.Any()) continue;

            await notificationService.SendAlertAsync(alert, notifications, cancellationToken);

            alert.LastNotification = notifications.Max(x => x.CreatedOn);

            await alertStore.UpdateAsync(alert, cancellationToken);
        }
    }
}