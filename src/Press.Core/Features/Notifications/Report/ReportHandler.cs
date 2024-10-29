using MediatR;
using Press.Core.Domain;
using Press.Core.Infrastructure;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.Notifications.Report;

public class ReportHandler(
    IUserStore userStore,
    IAlertStore alertStore,
    IPublicationStore publicationStore,
    INotificationService notificationService)
    : IRequestHandler<ReportRequest>
{
    public async Task Handle(ReportRequest request, CancellationToken cancellationToken)
    {
        var users = await userStore.GetAllAsync(cancellationToken);

        foreach (var user in users)
        {
            var report = new Dictionary<Domain.Alert, List<Publication>>();

            var alerts = await alertStore.GetByUserIdAsync(user.Id, cancellationToken);
            
            foreach (var alert in alerts)
            {
                var pubs = await publicationStore.SearchAsync(alert.Term, cancellationToken);

                if (!report.ContainsKey(alert))
                    report[alert] = [];
                
                report[alert].AddRange(pubs);
            }

            if (!report.Any()) continue;
            
            await notificationService.SendReportAsync(user, report, cancellationToken);
        }
    }
}