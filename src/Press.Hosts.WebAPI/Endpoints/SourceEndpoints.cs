using MediatR;
using Microsoft.AspNetCore.Mvc;
using Press.Core.Features.Sources.Get;

namespace Press.Hosts.WebAPI.Endpoints;

public static class SourceEndpoints
{
    public static WebApplication MapSourceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/sources");
        
        group.MapGet("/",
                async ([FromServices] IMediator mediator, CancellationToken cancellationToken)
                    => await mediator.Send(new GetSourcesRequest(), cancellationToken))
            .CacheOutput();

        return app;
    }
}