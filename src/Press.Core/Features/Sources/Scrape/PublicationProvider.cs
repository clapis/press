using Press.Core.Domain;
using Press.Core.Infrastructure.Scrapers;

namespace Press.Core.Features.Sources.Scrape;

public interface IPublicationProvider
{
    Task<List<Publication>> ProvideAsync(Source source, CancellationToken cancellationToken);
}

public class PublicationProvider(IEnumerable<ISourcePublicationProvider> providers) : IPublicationProvider
{
    public async Task<List<Publication>> ProvideAsync(Source source, CancellationToken cancellationToken)
    {
        var provider = GetProvider(source);

        return await provider.ProvideAsync(cancellationToken);
    }
    
    private ISourcePublicationProvider GetProvider(Source source)
    {
        var results =  providers.Where(x => x.SourceId == source.Id).ToList();

        if (results == null)
            throw new Exception($"Failed to find the provider for source '{source.Id}'");
        
        if (results.Count > 1)
            throw new Exception($"There are multiple providers registered with source '{source.Id}'");

        return results.Single();
    }
}