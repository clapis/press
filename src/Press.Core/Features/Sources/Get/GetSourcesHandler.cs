using MediatR;
using Press.Core.Domain;
using Press.Core.Infrastructure.Cache;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.Sources.Get;

public record GetSourcesRequest : IRequest<IReadOnlyCollection<GetSourcesResponseItem>>;
public record GetSourcesResponseItem(string Id, string Name, string Url, bool IsOfficial, DateTime? LastPublicationOn);

public class GetSourcesHandler(IPublicationStore publications, ICachedSourceStore cache) 
    : IRequestHandler<GetSourcesRequest, IReadOnlyCollection<GetSourcesResponseItem>>
{
    public async Task<IReadOnlyCollection<GetSourcesResponseItem>> Handle(GetSourcesRequest request, CancellationToken cancellationToken)
    {
        var sources = await cache.GetSourceMapAsync(cancellationToken);
        
        var latest = await publications.GetLatestBySourceAsync(cancellationToken);
        
        var summary = sources.Values
            .ToDictionary(source => source, source => latest.Single(x => x.SourceId == source.Id));
        
        return sources.Values.Select(source => Map(source, latest.SingleOrDefault(x => x.SourceId == source.Id))).ToList();
    }

    private GetSourcesResponseItem Map(Source source, Publication? latest)
        => new(source.Id, source.Name, source.Url, source.IsOfficial, latest?.Date);
}