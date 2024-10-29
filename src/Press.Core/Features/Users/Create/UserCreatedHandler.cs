using MediatR;
using Press.Core.Domain;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.Users.Create;

public record CreateUser(string Id, string Email) : IRequest;

public class UserCreatedHandler(IUserStore store) : IRequestHandler<CreateUser>
{
    public async Task Handle(CreateUser request, CancellationToken cancellationToken)
        => await store.InsertAsync(new User { Id = request.Id, Email = request.Email }, cancellationToken);
}