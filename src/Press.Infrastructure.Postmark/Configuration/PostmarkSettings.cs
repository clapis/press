namespace Press.Infrastructure.Postmark.Configuration;

public record PostmarkSettings
{
    public required string ApiToken { get; init; }
    public required string Sender { get; init; }
}