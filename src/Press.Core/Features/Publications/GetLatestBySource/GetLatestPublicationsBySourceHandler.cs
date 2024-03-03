using MediatR;
using Press.Core.Domain;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.Publications.GetLatestBySource;

public class GetLatestPublicationsBySourceHandler(IPublicationStore store) 
    : IRequestHandler<GetLatestPublicationsBySourceRequest, List<Publication>>
{
    public async Task<List<Publication>> Handle(GetLatestPublicationsBySourceRequest request, CancellationToken cancellationToken) 
        => await store.GetLatestPublicationsBySourceAsync(cancellationToken);
}