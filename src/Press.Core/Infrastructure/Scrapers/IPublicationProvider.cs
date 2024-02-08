using Press.Core.Domain;

namespace Press.Core.Infrastructure.Scrapers;

public interface IPublicationProvider
{
    IAsyncEnumerable<Publication> ProvideAsync(CancellationToken cancellationToken);
}