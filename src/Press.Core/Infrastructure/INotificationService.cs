using Press.Core.Domain;

namespace Press.Core.Infrastructure;

public interface INotificationService
{
    Task SendAlertAsync(User user, Alert alert, List<Publication> publications, CancellationToken cancellationToken);
    Task SendReportAsync(User user, Dictionary<Alert, List<Publication>> report, CancellationToken cancellationToken);
    Task NotifyStaleSourcesAsync(Dictionary<Source, DateTime> stale, CancellationToken cancellationToken);
}