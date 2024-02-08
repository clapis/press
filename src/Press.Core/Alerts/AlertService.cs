using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Press.Core.Publications;

namespace Press.Core.Alerts
{
    public class AlertService
    {
        private readonly IAlertStore _alertStore;
        private readonly IPublicationStore _publicationStore;
        private readonly INotificationService _notificationService;

        public AlertService(
            IAlertStore alertStore,
            IPublicationStore publicationStore,
            INotificationService notificationService)
        {
            _alertStore = alertStore;
            _publicationStore = publicationStore;
            _notificationService = notificationService;
        }

        public async Task AlertAsync(CancellationToken cancellationToken)
        {
            var date = await _publicationStore.GetMaxDateAsync(cancellationToken);
            var alerts = await _alertStore.GetAllAsync(cancellationToken);
            
            foreach (var alert in alerts)
            {
                var pubs = await _publicationStore.SearchAsync(alert.Term, cancellationToken);

                var notifications = pubs.Where(p => p.Date > alert.LastNotification).ToList();

                if (!notifications.Any()) continue;
                
                var info = new NotificationInfo(alert, notifications, date);
                    
                await _notificationService.SendAlertAsync(info, cancellationToken);

                alert.LastNotification = notifications.Max(x => x.Date);
                    
                await _alertStore.UpdateAsync(alert, cancellationToken);
            }
        }
    }
}