using System.Text;
using PostmarkDotNet;
using Press.Core.Domain;
using Press.Core.Infrastructure;
using Press.Infrastructure.Postmark.Configuration;

namespace Press.Infrastructure.Postmark.Services;

public class NotificationService : INotificationService
{
    private readonly PostmarkClient _client;
    private readonly PostmarkSettings _settings;

    public NotificationService(PostmarkSettings settings)
    {
        _settings = settings;
        _client = new PostmarkClient(_settings.ApiToken);
    }

    public async Task SendAlertAsync(Alert alert, List<Publication> publications, CancellationToken cancellationToken)
    {
        var email = alert.NotifyEmail;
        var subject = $"Alerta: {alert.Term}";

        var body = new StringBuilder();
        body.AppendLine($"Termo '{alert.Term}' encontrado em:");
        body.AppendLine();
        publications.ForEach(pub => body.AppendLine($"{pub.Date:dd/MM/yyyy}: {pub.Url}"));
        body.AppendLine();
        body.AppendLine("Ciao, :)");

        await NotifyAsync(email, subject, body.ToString(), cancellationToken);
    }

    public async Task SendReportAsync(string email, Dictionary<Alert, List<Publication>> report, CancellationToken cancellationToken)
    {
        var subject = "Resumo semanal";

        var body = new StringBuilder();

        body.AppendLine("Últimos resultados encontrados para os termos:");
        body.AppendLine();

        foreach (var (alert, publications) in report)
        {
            body.AppendLine($"## {alert.Term}");
            body.AppendLine();
            publications.ForEach(pub => body.AppendLine($"{pub.Date:dd/MM/yyyy}: {pub.Url}"));
            body.AppendLine();
            body.AppendLine();
        }
        
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