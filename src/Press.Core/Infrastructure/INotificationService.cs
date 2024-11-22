using Press.Core.Domain;

namespace Press.Core.Infrastructure;

public interface INotificationService
{
    Task SendAlertAsync(User user, Alert alert, List<Publication> publications, CancellationToken cancellationToken);
    Task ReportStaleSourcesAsync(Dictionary<Source, DateTime> stale, CancellationToken cancellationToken);
    Task ReportSourcesStatusAsync(Dictionary<Source, Publication> summary, CancellationToken cancellationToken);
}