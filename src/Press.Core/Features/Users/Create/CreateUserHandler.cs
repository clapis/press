using MediatR;
using Press.Core.Domain;
using Press.Core.Infrastructure.Data;
using Press.Core.Infrastructure.Subscriptions;

namespace Press.Core.Features.Users.Create;

public record CreateUser(string Id, string Email) : IRequest;

public class CreateUserHandler(
    IUserStore store,
    IStripeService service) : IRequestHandler<CreateUser>
{
    public async Task Handle(CreateUser request, CancellationToken cancellationToken)
    {
        var customerId = await service.RegisterCustomerAsync(request.Email, cancellationToken);

        var user = new User
        {
            Id = request.Id, 
            Email = request.Email, 
            CustomerId = customerId,
            Subscription = new ()
            {
                Name = "Explore Grátis",
                MaxAlerts = 3,
                IsTrial = true
            }
        };

        await store.InsertAsync(user, cancellationToken);
    }
}