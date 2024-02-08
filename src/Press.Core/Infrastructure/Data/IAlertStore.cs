using Press.Core.Domain;

namespace Press.Core.Infrastructure.Data;

public interface IAlertStore
{
    Task<List<Alert>> GetAllAsync(CancellationToken cancellationToken);
    Task InsertAsync(Alert alert, CancellationToken cancellationToken);
    Task UpdateAsync(Alert alert, CancellationToken cancellationToken);
}