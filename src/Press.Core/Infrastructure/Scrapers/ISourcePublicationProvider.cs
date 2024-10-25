using Press.Core.Domain;

namespace Press.Core.Infrastructure.Scrapers;

public interface ISourcePublicationProvider
{
    string SourceId { get; }

    Task<List<Publication>> ProvideAsync(CancellationToken cancellationToken);
}