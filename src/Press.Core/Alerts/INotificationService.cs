using Press.Core.Publications;

namespace Press.Core.Alerts
{
    public interface INotificationService
    {
        Task SendAlertAsync(NotificationInfo info, CancellationToken cancellationToken);
        Task SendReportAsync(NotificationInfo info, CancellationToken cancellationToken);
    }

    public record NotificationInfo(Alert Alert, List<Publication> Publications);
}