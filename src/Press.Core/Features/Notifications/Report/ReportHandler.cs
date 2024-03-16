using MediatR;
using Press.Core.Domain;
using Press.Core.Infrastructure;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.Notifications.Report;

public class ReportHandler(
    IAlertStore alertStore,
    IPublicationStore publicationStore,
    INotificationService notificationService)
    : IRequestHandler<ReportRequest>
{
    public async Task Handle(ReportRequest request, CancellationToken cancellationToken)
    {
        var alerts = await alertStore.GetAllAsync(cancellationToken);

        foreach (var alertsByNotificationEmail in alerts.GroupBy(x => x.NotifyEmail))
        {
            var email = alertsByNotificationEmail.Key;
            var report = new Dictionary<Domain.Alert, List<Publication>>();
            
            foreach (var alert in alertsByNotificationEmail)
            {
                var pubs = await publicationStore.SearchAsync(alert.Term, cancellationToken);

                if (!report.ContainsKey(alert))
                    report[alert] = [];
                
                pubs.AddRange(pubs);
            }

            if (!report.Any()) continue;
            
            await notificationService.SendReportAsync(email, report, cancellationToken);
        }
    }
}