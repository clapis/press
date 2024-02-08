using Press.Core.Domain;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.Publications.GetLatestBySource;

public class GetLatestPublicationsBySourceHandler(IPublicationStore store)
{
    public async Task<List<Publication>> HandleAsync(CancellationToken cancellationToken) 
        => await store.GetLatestPublicationsBySourceAsync(cancellationToken);
}