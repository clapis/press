using MediatR;
using Press.Core.Infrastructure;
using Press.Core.Infrastructure.Cache;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.System;

public record ReportStaleSources : IRequest;

public class ReportStaleSourcesHandler(
    ICachedSourceStore sourceStore,
    IPublicationStore publicationStore, 
    INotificationService notificationService) 
    : IRequestHandler<ReportStaleSources>
{
    private static readonly TimeSpan MaxPublicationAge = TimeSpan.FromDays(3);
    
    public async Task Handle(ReportStaleSources delayRequest, CancellationToken cancellationToken)
    {
        var sources = await sourceStore.GetSourceMapAsync(cancellationToken);
        var latest = await publicationStore.GetLatestBySourceAsync(cancellationToken);

        var stale = latest
            .Where(x => DateTime.UtcNow - x.CreatedOn > MaxPublicationAge)
            .ToDictionary(pub => sources[pub.SourceId], pub => pub.Date);

        if (!stale.Any()) return;

        await notificationService.ReportStaleSourcesAsync(stale, cancellationToken);
    }
}