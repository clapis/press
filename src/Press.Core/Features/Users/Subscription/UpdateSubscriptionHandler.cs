using MediatR;
using Press.Core.Infrastructure.Data;
using Press.Core.Infrastructure.Subscriptions;

namespace Press.Core.Features.Users.Subscription;

public record UpdateSubscription(string CustomerId) : IRequest;

public class UpdateSubscriptionHandler(
    IUserStore store,
    ISubscriptionService service) 
    : IRequestHandler<UpdateSubscription>
{
    public async Task Handle(UpdateSubscription request, CancellationToken cancellationToken)
    {
        var user = await store.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
        
        if (user == null)
            throw new Exception($"User with customer id {request.CustomerId} was not found");
        
        var subscription = await service.GetSubscriptionAsync(user.CustomerId, cancellationToken);
        
        await store.UpdateSubscriptionAsync(user.Id, subscription, cancellationToken);
    }
}