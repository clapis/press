using Press.Core.Domain;

namespace Press.Core.Infrastructure.Data;

public interface IAlertStore
{
    Task<List<Alert>> GetAllAsync(CancellationToken cancellationToken);
    Task<List<Alert>> GetByUserIdAsync(string userId, CancellationToken cancellationToken);
    Task InsertAsync(Alert alert, CancellationToken cancellationToken);
    Task DeleteAsync(string id, string userId, CancellationToken cancellationToken);
    Task UpdateAsync(Alert alert, CancellationToken cancellationToken);
}