using Press.Core.Domain;

namespace Press.Core.Infrastructure.Scrapers;

public interface IPublicationProvider
{
    PublicationSource Source { get; }
    
    Task<List<Publication>> ProvideAsync(CancellationToken cancellationToken);
}