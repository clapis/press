using Press.Core.Domain;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.Publications.Search;

public class PublicationsSearchHandler(IPublicationStore store)
{
    public async Task<List<Publication>> HandleAsync(string keyword, CancellationToken cancellationToken) 
        => await store.SearchAsync(keyword, cancellationToken);
}