using MediatR;
using Microsoft.AspNetCore.Mvc;
using Press.Core.Features.Sources.Scrape;

namespace Press.Hosts.WebAPI.Endpoints;

public static class SystemEndpoints
{
    public static WebApplication MapSystemEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/system");
        
        group.MapPost("/scrape/{id}",
            async ([FromServices] IMediator mediator, string id, CancellationToken cancellationToken)
                => await mediator.Send(new ScrapeSourcesRequest { Ids = [id] }, cancellationToken));

        return app;
    }
}