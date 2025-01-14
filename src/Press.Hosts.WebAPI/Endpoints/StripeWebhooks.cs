using MediatR;
using Microsoft.AspNetCore.Mvc;
using Press.Core.Features.Users.Subscription;
using Press.Infrastructure.Stripe;
using Stripe;

namespace Press.Hosts.WebAPI.Endpoints;

public static class StripeWebhooks
{
    public static WebApplication MapStripeWebhooks(this WebApplication app)
    {
        app.MapPost("/webhooks/stripe",
            async (HttpContext context,
                [FromServices] IMediator mediator,
                [FromServices] StripeSettings settings, 
                [FromServices] ILogger<StripeException> logger) =>
            {
                var json = await new StreamReader(context.Request.Body).ReadToEndAsync();

                var @event = EventUtility.ConstructEvent(json, context.Request.Headers["Stripe-Signature"], settings.WebhookSecret);

                switch (@event.Type)
                {
                    case EventTypes.CustomerSubscriptionCreated:
                    case EventTypes.CustomerSubscriptionUpdated:
                    case EventTypes.CustomerSubscriptionDeleted:
                        var subscription = (Subscription)@event.Data.Object;
                        await mediator.Send(new UpdateSubscription(subscription.CustomerId));
                        break;
                    default:
                        logger.LogWarning("Unhandled Stripe webhook event {EventType}", @event.Type);
                        break;
                }

                return Results.Ok();
            });

        return app;
    }
}