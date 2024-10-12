namespace Press.Infrastructure.Postmark.Configuration;

public record PostmarkSettings
{
    public required bool IsEnabled { get; set; }
    public required string ApiToken { get; init; }
    public required string Sender { get; init; }
}