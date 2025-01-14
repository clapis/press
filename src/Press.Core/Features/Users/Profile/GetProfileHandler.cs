using MediatR;
using Press.Core.Domain;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.Users.Profile;

public record GetProfile(string UserId) : IRequest<User>;

public class GetProfileHandler(IUserStore store) : IRequestHandler<GetProfile, User>
{
    public async Task<User> Handle(GetProfile request, CancellationToken cancellationToken) 
        => await store.GetByIdAsync(request.UserId, cancellationToken);
}