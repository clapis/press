using MediatR;
using Press.Core.Domain;
using Press.Core.Infrastructure.Cache;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.Publications.Search;

public record SearchPublicationsRequest(string Query) : IRequest<IReadOnlyCollection<SearchPublicationsResponseItem>>;
public record SearchPublicationsResponseItem(string Id, string Url, string Source, DateTime Date);

public class SearchPublicationsHandler(IPublicationStore publications, ICachedSourceStore cache)
    : IRequestHandler<SearchPublicationsRequest, IReadOnlyCollection<SearchPublicationsResponseItem>>
{
    public async Task<IReadOnlyCollection<SearchPublicationsResponseItem>> Handle(SearchPublicationsRequest request, CancellationToken cancellationToken)
    {
        var sources = await cache.GetSourceMapAsync(cancellationToken);
        var results = await publications.SearchAsync(request.Query,  null, cancellationToken);

        return results.Select(x => Map(x, sources[x.SourceId])).ToList();
    }
    
    private SearchPublicationsResponseItem Map(Publication publication, Source source)
        => new (publication.Id, publication.Url, source.Name, publication.Date);
}