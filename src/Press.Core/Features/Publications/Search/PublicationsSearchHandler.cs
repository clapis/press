using MediatR;
using Press.Core.Domain;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.Publications.Search;

public class PublicationsSearchHandler(IPublicationStore store) 
    : IRequestHandler<PublicationsSearchRequest, List<Publication>>
{
    public async Task<List<Publication>> Handle(PublicationsSearchRequest request, CancellationToken cancellationToken) 
        => await store.SearchAsync(request.Query, cancellationToken);
}