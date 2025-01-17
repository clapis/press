using MediatR;
using Press.Core.Infrastructure;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.Alerts.Run;

public record RunAlerts : IRequest;

public class RunAlertsHandler(
    IUserStore userStore,
    IAlertStore alertStore,
    IPublicationStore publicationStore,
    INotificationService notificationService)
    : IRequestHandler<RunAlerts>
{
    public async Task Handle(RunAlerts request, CancellationToken cancellationToken)
    {
        var users = await userStore.GetAllAsync(cancellationToken);

        foreach (var user in users)
        {
            var alerts = await alertStore.GetByUserIdAsync(user.Id, cancellationToken);

            foreach (var alert in alerts)
            {
                var pubs = await publicationStore.SearchAsync(alert.Keyword, alert.SourceId, cancellationToken);
                
                var last = alert.LastNotification ?? DateTime.MinValue;

                var notifications = pubs.Where(p => p.CreatedOn > last).ToList();

                if (!notifications.Any()) continue;
                
                await notificationService.SendAlertAsync(user, alert, notifications, cancellationToken);

                alert.LastNotification = notifications.Max(x => x.CreatedOn);

                await alertStore.UpdateAsync(alert, cancellationToken);
            }
        }
    }
}