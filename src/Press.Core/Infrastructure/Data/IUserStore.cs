using Press.Core.Domain;

namespace Press.Core.Infrastructure.Data;

public interface IUserStore
{
    Task<List<User>> GetAllAsync(CancellationToken cancellationToken);
    Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task InsertAsync(User user, CancellationToken cancellationToken);
}