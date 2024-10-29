using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Press.Core.Features.Alerts.Create;
using Press.Core.Features.Alerts.Delete;
using Press.Core.Features.Alerts.Get;
using Press.Hosts.WebAPI.Extensions;

namespace Press.Hosts.WebAPI.Endpoints;

public static class AlertEndpoints
{
    public static WebApplication MapAlertEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/alerts");

        group.RequireAuthorization();

        group.MapGet("/", async ([FromServices] IMediator mediator, ClaimsPrincipal principal, CancellationToken cancellationToken)
            => await mediator.Send(new GetAlertsRequest(principal.GetUserId()), cancellationToken));
        
        group.MapPost("/", async ([FromBody] CreateAlertModel model, [FromServices] IMediator mediator, ClaimsPrincipal principal, CancellationToken cancellationToken)
            => await mediator.Send(new CreateAlertRequest(model.Keyword, principal.GetUserId()), cancellationToken));
        
        group.MapDelete("/{id}", async (string id, [FromServices] IMediator mediator, ClaimsPrincipal principal, CancellationToken cancellationToken)
            => await mediator.Send(new DeleteAlertRequest(id, principal.GetUserId()), cancellationToken));

        return app;
    }

    record CreateAlertModel(string Keyword);
}