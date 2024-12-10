using MediatR;
using Press.Core.Domain;
using Press.Core.Infrastructure.Cache;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.Publications.Latest;

public record GetLatestPublicationsBySourceRequest : IRequest<IReadOnlyCollection<GetLatestPublicationsBySourceResponseItem>>;
public record GetLatestPublicationsBySourceResponseItem(string Id, string Url, string Source, DateTime Date, bool IsOfficial);

public class GetLatestPublicationsBySourceHandler(IPublicationStore publications, ICachedSourceStore cache) 
    : IRequestHandler<GetLatestPublicationsBySourceRequest, IReadOnlyCollection<GetLatestPublicationsBySourceResponseItem>>
{
    public async Task<IReadOnlyCollection<GetLatestPublicationsBySourceResponseItem>> Handle(GetLatestPublicationsBySourceRequest request, CancellationToken cancellationToken)
    {
        var sources = await cache.GetSourceMapAsync(cancellationToken);
        var results = await publications.GetLatestBySourceAsync(cancellationToken);
        
        return results.Select(x => Map(x, sources[x.SourceId])).ToList();
    }

    private GetLatestPublicationsBySourceResponseItem Map(Publication publication, Source source)
        => new(publication.Id, publication.Url, source.Name, publication.Date, source.IsOfficial);
}