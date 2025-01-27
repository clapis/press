using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PostmarkDotNet;
using Press.Core.Domain;
using Press.Core.Infrastructure;

namespace Press.Infrastructure.Postmark.Services;

public class NotificationService : INotificationService
{
    private readonly PostmarkClient _client;
    private readonly PostmarkSettings _settings;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        PostmarkSettings settings, 
        ILogger<NotificationService> logger)
    {
        _logger = logger;
        _settings = settings;
        _client = new PostmarkClient(_settings.ApiToken);
    }

    public async Task SendAlertAsync(User user, Alert alert, List<Publication> publications, CancellationToken cancellationToken)
    {
        var subject = $"{alert.Keyword}";

        var body = new StringBuilder();
        body.AppendLine($"Termo '{alert.Keyword}' encontrado em:");
        body.AppendLine();
        publications.ForEach(pub => body.AppendLine($"{pub.Date:dd/MM/yyyy}: {pub.Url}"));
        body.AppendLine();
        body.AppendLine("Até mais, :)");

        await NotifyAsync(user.Email, subject, body.ToString(), cancellationToken);
    }

    public async Task ReportStaleSourcesAsync(Dictionary<Source, DateTime> stale, CancellationToken cancellationToken)
    {
        var subject = "System: Stale Sources";

        var body = new StringBuilder();
        body.AppendLine();
        foreach (var (source, last) in stale)
        {
            body.AppendLine($"{source.Name}: {last:dd/MM/yyyy}");
            body.AppendLine(source.Url);
            body.AppendLine();
        }
        body.AppendLine();
        body.AppendLine("Ciao, :)");

        await NotifyAsync(_settings.Sender, subject, body.ToString(), cancellationToken);
    }

    public async Task ReportSourcesStatusAsync(Dictionary<Source, Publication?> summary, CancellationToken cancellationToken)
    {
        var subject = "System: Source Status";

        var body = new StringBuilder();

        foreach (var (source, last) in summary)
        {
            body.AppendLine($"{source.Name}");
            body.AppendLine($"IsEnabled: {source.IsEnabled}");
            body.AppendLine(last == null ? "Last Publication: N/A" : $"Last Publication: {last.Date:dd/MM/yyyy} ({(DateTime.Now - last.Date).Days} days ago)");
            body.AppendLine($"Url: {source.Url}");
            body.AppendLine();
        }
        body.AppendLine();
        body.AppendLine("Ciao, :)");

        await NotifyAsync(_settings.Sender, subject, body.ToString(), cancellationToken);
    }

    private async Task NotifyAsync(string email, string subject, string body, CancellationToken cancellationToken)
    {
        var msg = new PostmarkMessage
        {
            To = email,
            From = _settings.Sender,
            Subject = $"{subject}",
            TextBody = body
        };
        
        if (!_settings.IsEnabled)
        {
            _logger.LogInformation("Postmark disabled. {PostmarkMessage}",JsonSerializer.Serialize(msg));
            return;
        }

        var response = await _client.SendMessageAsync(msg);

        if (response.Status != PostmarkStatus.Success)
            throw new Exception($"{response.Status}: {response.Message} ({response.ErrorCode})");
    }
}