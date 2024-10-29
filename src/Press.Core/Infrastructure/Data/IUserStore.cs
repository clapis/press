using Press.Core.Domain;

namespace Press.Core.Infrastructure.Data;

public interface IUserStore
{
    Task InsertAsync(User user, CancellationToken cancellationToken);
}