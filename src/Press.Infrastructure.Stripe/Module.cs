using Microsoft.Extensions.DependencyInjection;
using Press.Core.Infrastructure.Subscriptions;
using Stripe;
using Stripe.BillingPortal;

namespace Press.Infrastructure.Stripe;

public static class Module
{
    public static IServiceCollection AddStripe(this IServiceCollection services, StripeSettings settings)
    {
        StripeConfiguration.EnableTelemetry = false;
        StripeConfiguration.ApiKey = settings.ApiKey;
        
        services.AddSingleton(settings);
        
        services
            .AddTransient<ProductService>()
            .AddTransient<CustomerService>()
            .AddTransient<SessionService>()
            .AddTransient<CustomerSessionService>();

        services.AddTransient<ISubscriptionService, Services.SubscriptionService>();
        
        return services;
    }
}