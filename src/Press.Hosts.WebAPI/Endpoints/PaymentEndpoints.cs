using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Press.Core.Infrastructure.Data;
using Press.Core.Infrastructure.Subscriptions;
using Press.Hosts.WebAPI.Extensions;

namespace Press.Hosts.WebAPI.Endpoints;

public static class PaymentEndpoints
{
    public static WebApplication MapPaymentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/payments");
        
        group.RequireAuthorization();

        group.MapPost("/pricing",
            async (ClaimsPrincipal principal,
                [FromServices] IUserStore store,
                [FromServices] IStripeService service,
                CancellationToken cancellationToken) =>
            {
                var user = await store.GetByIdAsync(principal.GetUserId(), cancellationToken);

                return await service.CreatePricingSessionSecretAsync(user.CustomerId, cancellationToken);
            });

        group.MapPost("/portal",
            async (ClaimsPrincipal principal,
                [FromServices] IUserStore store,
                [FromServices] IStripeService service,
                CancellationToken cancellationToken) =>
            {
                var user = await store.GetByIdAsync(principal.GetUserId(), cancellationToken);

                return await service.CreateBillingPortalSessionAsync(user.CustomerId, cancellationToken);
            });

        return app;
    }
}