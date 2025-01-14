using Press.Core.Infrastructure.Subscriptions;
using Stripe;
using Stripe.BillingPortal;
using Subscription = Press.Core.Domain.Subscription;

namespace Press.Infrastructure.Stripe.Services;

public class StripeService(
    SessionService sessionService,
    ProductService productService,
    CustomerService customerService,
    CustomerSessionService customerSessionService) : IStripeService
{
    private const string DefaultLocale = "pt-BR";
    
    private static readonly string[] ActiveSubscriptionStatuses = [ SubscriptionStatuses.Active, 
        SubscriptionStatuses.Incomplete, SubscriptionStatuses.Unpaid, SubscriptionStatuses.Paused ];

    public async Task<string> RegisterCustomerAsync(string email, CancellationToken cancellationToken)
    {
        var opts = new CustomerCreateOptions { Email = email, PreferredLocales = [ DefaultLocale ]};

        var customer = await customerService.CreateAsync(opts, cancellationToken: cancellationToken);
        
        return customer.Id;
    }

    public async Task<Subscription?> GetSubscriptionAsync(string customerId, CancellationToken cancellationToken)
    {
        var opts = new CustomerGetOptions
        {
            Expand = ["subscriptions.data.items.data"]
        };
        
        var customer = await customerService.GetAsync(customerId, opts, cancellationToken: cancellationToken);
        
        var subscription = customer.Subscriptions
            .Where(x => ActiveSubscriptionStatuses.Contains(x.Status))
            .OrderByDescending(x => x.Created)
            .FirstOrDefault();

        if (subscription != null)
        {
            foreach (var item in subscription.Items)
            {
                var product = await productService.GetAsync(item.Price.ProductId, cancellationToken: cancellationToken);

                if (product.Metadata.TryGetValue("max_alerts", out var max))
                {
                    return new Subscription
                    {
                        Name = product.Name,
                        MaxAlerts = int.Parse(max),
                        IsTrial = false
                    };
                }
            }
        }

        return default;
    }

    public async Task<string> CreatePricingSessionSecretAsync(string customerId, CancellationToken cancellationToken)
    {
        var options = new CustomerSessionCreateOptions
        {
            Customer = customerId,
            Components = new CustomerSessionComponentsOptions
            {
                PricingTable = new CustomerSessionComponentsPricingTableOptions
                {
                    Enabled = true
                }
            }
        };
        
        var session = await customerSessionService.CreateAsync(options, cancellationToken: cancellationToken);

        return session.ClientSecret;
    }

    public async Task<string> CreateBillingPortalSessionAsync(string customerId, CancellationToken cancellationToken)
    {
        var options = new SessionCreateOptions
        {
            Customer = customerId,
            Locale = DefaultLocale,
        };
        
        var session = await sessionService.CreateAsync(options, cancellationToken: cancellationToken);

        return session.Url;
    }
}