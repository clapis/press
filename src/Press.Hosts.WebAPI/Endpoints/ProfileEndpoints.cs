using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Press.Core.Features.Users.Profile;
using Press.Hosts.WebAPI.Extensions;

namespace Press.Hosts.WebAPI.Endpoints;

public static class UserEndpoints
{
    public static WebApplication MapProfileEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/profile");
        
        group.RequireAuthorization();

        group.MapGet("/",
            async ([FromServices] IMediator mediator, ClaimsPrincipal principal, CancellationToken cancellationToken)
                => await mediator.Send(new GetProfile(principal.GetUserId()), cancellationToken));

        return app;
    }
}