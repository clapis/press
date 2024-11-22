using MediatR;
using Press.Core.Infrastructure;
using Press.Core.Infrastructure.Cache;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.System;

public record ReportSourcesStatus : IRequest;

public class ReportSourcesStatusHandler(
    ICachedSourceStore sourceStore, 
    IPublicationStore publicationStore,
    INotificationService notificationService) 
    : IRequestHandler<ReportSourcesStatus>
{
    public async Task Handle(ReportSourcesStatus request, CancellationToken cancellationToken)
    {
        var sources = await sourceStore.GetSourceMapAsync(cancellationToken);
        var latest = await publicationStore.GetLatestBySourceAsync(cancellationToken);
        
        var summary = sources.Values
            .ToDictionary(source => source, source => latest.Single(x => x.SourceId == source.Id));

        await notificationService.ReportSourcesStatusAsync(summary, cancellationToken);
    }
}