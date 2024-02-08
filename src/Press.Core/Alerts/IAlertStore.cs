using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Press.Core.Alerts
{
    public interface IAlertStore
    {
        Task<List<Alert>> GetAllAsync(CancellationToken cancellationToken);
        Task InsertAsync(Alert alert, CancellationToken cancellationToken);
        Task UpdateAsync(Alert alert, CancellationToken cancellationToken);
    }
}