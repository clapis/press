using Press.Core.Publications;

namespace Press.Core.Alerts
{
    public class ReportService
    {
        private readonly IAlertStore _alertStore;
        private readonly IPublicationStore _publicationStore;
        private readonly INotificationService _notificationService;

        public ReportService(
            IAlertStore alertStore,
            IPublicationStore publicationStore,
            INotificationService notificationService)
        {
            _alertStore = alertStore;
            _publicationStore = publicationStore;
            _notificationService = notificationService;
        }

        public async Task ReportAsync(CancellationToken cancellationToken)
        {
            var date = await _publicationStore.GetMaxDateAsync(cancellationToken);
            var alerts = await _alertStore.GetAllAsync(cancellationToken);
            
            foreach (var alert in alerts)
            {
                var pubs = await _publicationStore.SearchAsync(alert.Term, cancellationToken);
            
                if (pubs.Any())
                {
                    var info = new NotificationInfo(alert, pubs, date);
                    await _notificationService.SendReportAsync(info, cancellationToken);
                }
            }
        }
    }
}