using MediatR;
using Press.Core.Infrastructure;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.Alerts.Report;

public class ReportHandler(
    IAlertStore alertStore,
    IPublicationStore publicationStore,
    INotificationService notificationService)
    : IRequestHandler<ReportRequest>
{
    public async Task Handle(ReportRequest request, CancellationToken cancellationToken)
    {
        var alerts = await alertStore.GetAllAsync(cancellationToken);

        foreach (var alert in alerts)
        {
            var pubs = await publicationStore.SearchAsync(alert.Term, cancellationToken);

            if (!pubs.Any()) continue;

            var info = new NotificationInfo(alert, pubs);

            await notificationService.SendReportAsync(info, cancellationToken);
        }
    }
}