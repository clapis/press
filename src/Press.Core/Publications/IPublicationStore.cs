using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Press.Core.Publications
{
    public interface IPublicationStore
    {
        Task<DateTime> GetMaxDateAsync(CancellationToken cancellationToken);
        Task<List<string>> GetAllUrlsAsync(CancellationToken cancellationToken);
        Task SaveAsync(Publication publication, CancellationToken cancellationToken);
        Task<List<Publication>> SearchAsync(string query, CancellationToken cancellationToken);
    }
}