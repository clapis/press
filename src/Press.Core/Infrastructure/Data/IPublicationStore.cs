using Press.Core.Domain;

namespace Press.Core.Infrastructure.Data;

public interface IPublicationStore
{
    Task SaveAsync(Publication publication, CancellationToken cancellationToken);
    Task<List<Publication>> SearchAsync(string query, CancellationToken cancellationToken);
    Task<List<Publication>> GetLatestBySourceAsync(CancellationToken cancellationToken);
    Task<List<string>> GetLatestUrlsAsync(Source source, CancellationToken cancellationToken);
}