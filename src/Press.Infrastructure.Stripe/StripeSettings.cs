namespace Press.Infrastructure.Stripe;

public class StripeSettings
{
    public required string ApiKey { get; init; }
    public required string WebhookSecret { get; init; }
}