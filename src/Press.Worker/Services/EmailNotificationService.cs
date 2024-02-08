using System.Text;
using PostmarkDotNet;
using Press.Core.Alerts;
using Press.Worker.Configuration;

namespace Press.Worker.Services
{
    public class EmailNotificationService : INotificationService
    {
        private readonly PostmarkClient _client;
        private readonly PostMarkSettings _settings;

        public EmailNotificationService(PostMarkSettings settings)
        {
            _settings = settings;
            _client = new PostmarkClient(_settings.ApiToken);
        }
        
        public async Task SendAlertAsync(NotificationInfo info, CancellationToken cancellationToken)
        {
            var email = info.Alert.NotifyEmail;
            var subject = $"Alerta: {info.Alert.Term}";
            
            var body = new StringBuilder();
            body.AppendLine($"Termo '{info.Alert.Term}' encontrado em:");
            body.AppendLine();
            info.Publications.ForEach(pub => body.AppendLine($"{pub.Date:dd/MM/yyyy}: {pub.Url}"));
            body.AppendLine();
            body.AppendLine("Ciao, :)");

            await NotifyAsync(email, subject, body.ToString(), cancellationToken);
        }

        public async Task SendReportAsync(NotificationInfo info, CancellationToken cancellationToken)
        {
            var email = info.Alert.NotifyEmail;
            var subject = $"Resumo semanal: {info.Alert.Term}";
            
            var body = new StringBuilder();
            body.AppendLine($"Termo '{info.Alert.Term}' encontrado em:");
            body.AppendLine();
            info.Publications.ForEach(pub => body.AppendLine($"{pub.Date:dd/MM/yyyy}: {pub.Url}"));
            body.AppendLine();
            body.AppendLine("Ciao, :)");

            await NotifyAsync(email, subject, body.ToString(), cancellationToken);
        }

        private async Task NotifyAsync(string email, string subject, string body, CancellationToken cancellationToken)
        {
            var msg = new PostmarkMessage
            {
                To = email,
                From = _settings.Sender,
                Subject = $"[Press] {subject}",
                TextBody = body
            };

            var response = await _client.SendMessageAsync(msg);

            if (response.Status != PostmarkStatus.Success)
                throw new Exception($"{response.Status}: {response.Message} ({response.ErrorCode})");
        }
    }
}