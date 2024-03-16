using Press.Core.Domain;

namespace Press.Core.Infrastructure;

public interface INotificationService
{
    Task SendAlertAsync(Alert alert, List<Publication> publications, CancellationToken cancellationToken);
    Task SendReportAsync(string email, Dictionary<Alert, List<Publication>> report, CancellationToken cancellationToken);
    Task SendMonitorAsync(List<Publication> delayed, CancellationToken cancellationToken);
}