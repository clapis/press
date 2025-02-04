using MediatR;
using Microsoft.AspNetCore.Mvc;
using Press.Core.Features.Publications.Search;

namespace Press.Hosts.WebAPI.Endpoints;

public static class PublicationEndpoints
{
    public static WebApplication MapPublicationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/publications");
        
        group.MapGet("/search",
            async ([FromServices] IMediator mediator, [FromQuery(Name = "q")] string query, CancellationToken cancellationToken)
                => await mediator.Send(new SearchPublicationsRequest(query), cancellationToken));

        return app;
    }
}