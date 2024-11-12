using MediatR;
using Microsoft.AspNetCore.Mvc;
using Press.Core.Features.Sources.Scrape;

namespace Press.Hosts.WebAPI.Endpoints;

public static class SystemEndpoints
{
    public static WebApplication MapSystemEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/system");
        
        group.MapPost("/scrape",
            async ([FromServices] IMediator mediator, CancellationToken cancellationToken)
                => await mediator.Send(new ScrapeSourcesRequest(), cancellationToken));

        return app;
    }

}