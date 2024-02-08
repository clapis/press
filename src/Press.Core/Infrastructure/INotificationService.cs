using Press.Core.Domain;

namespace Press.Core.Infrastructure;

public interface INotificationService
{
    Task SendAlertAsync(NotificationInfo info, CancellationToken cancellationToken);
    Task SendReportAsync(NotificationInfo info, CancellationToken cancellationToken);
}

public record NotificationInfo(Alert Alert, List<Publication> Publications);