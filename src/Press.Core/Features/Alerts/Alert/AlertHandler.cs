using Press.Core.Infrastructure;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.Alerts.Alert;

public class AlertHandler(
    IAlertStore alertStore,
    IPublicationStore publicationStore,
    INotificationService notificationService)
{
    public async Task HandleAsync(CancellationToken cancellationToken)
    {
        var alerts = await alertStore.GetAllAsync(cancellationToken);

        foreach (var alert in alerts)
        {
            var pubs = await publicationStore.SearchAsync(alert.Term, cancellationToken);

            var notifications = pubs.Where(p => p.CreatedOn > alert.LastNotification).ToList();

            if (!notifications.Any()) continue;

            var info = new NotificationInfo(alert, notifications);

            await notificationService.SendAlertAsync(info, cancellationToken);

            alert.LastNotification = notifications.Max(x => x.CreatedOn);

            await alertStore.UpdateAsync(alert, cancellationToken);
        }
    }
}