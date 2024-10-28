using MediatR;
using Press.Core.Domain;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.Sources.Get;

public record GetSourcesRequest : IRequest<List<Source>>;

public class GetSourcesHandler(ISourceStore store) : IRequestHandler<GetSourcesRequest, List<Source>>
{
    public async Task<List<Source>> Handle(GetSourcesRequest request, CancellationToken cancellationToken) 
        => await store.GetAllAsync(cancellationToken);
}