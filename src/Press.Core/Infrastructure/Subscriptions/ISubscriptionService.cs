using Press.Core.Domain;

namespace Press.Core.Infrastructure.Subscriptions;

public interface ISubscriptionService
{
    Task<string> RegisterCustomerAsync(string email, CancellationToken cancellationToken);
    Task<Subscription?> GetSubscriptionAsync(string customerId, CancellationToken cancellationToken);
    
    Task<string> CreatePricingSessionSecretAsync(string customerId, CancellationToken cancellationToken);
    Task<string> CreateBillingPortalSessionAsync(string customerId, CancellationToken cancellationToken);
}