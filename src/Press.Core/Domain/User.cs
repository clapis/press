namespace Press.Core.Domain;

public class User
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string CustomerId { get; set; }
    public Subscription? Subscription { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}