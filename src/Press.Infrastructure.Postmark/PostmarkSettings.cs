namespace Press.Infrastructure.Postmark;

public record PostmarkSettings
{
    public required bool IsEnabled { get; set; }
    public required string ApiToken { get; init; }
    public required string Sender { get; init; }
}